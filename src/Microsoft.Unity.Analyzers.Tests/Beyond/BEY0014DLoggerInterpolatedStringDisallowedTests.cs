using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BEY0014DLoggerInterpolatedStringDisallowedTests : BaseCodeFixVerifierTest<BEY0014DLoggerInterpolatedStringDisallowedAnalyzer, BEY0014DLoggerInterpolatedStringDisallowedCodeFix>
{
	[Fact]
	public async Task TestNotReported()
	{
		const string test = @"
namespace UnityEngine;

class Camera : UnityEngine.MonoBehaviour
{
    void Start()
    {
        int var1 = 123;
        string var2 = ""456"";
        Debug.Log(""Hello, world!"");
        Debug.LogWarning($""Hello, world! {var1} {var2}"");
        Debug.LogError($""Hello, world! {var1} {var2}"");
    }
}
";

		await VerifyCSharpDiagnosticAsync(test);
	}

	[Fact]
	public async Task TestIsReported()
	{
		const string test = @"
using UnityEngine;
using Beyond;

namespace Beyond
{
    enum ELogChannel { None }
    enum EColorTag { None }

    static class DLogger
    {
        public static void Log(string message, params object[] args) {}
        public static void LogWarning(UnityEngine.Object context, string message, params object[] args) {}
        public static void LogError(ELogChannel channel, EColorTag color, string message, params object[] args) {}
    }
}

class Camera : MonoBehaviour
{
    void Start()
    {
        int var1 = 123;
        string var2 = ""456"";
        DLogger.Log($""Hello, \""world\""! {var1} {var2}"");
        DLogger.LogWarning(this, $""Hello, world! {var1} {var2}"");
        DLogger.LogError(ELogChannel.None, EColorTag.None, $""Hello, world! {var1} {var2}"");
    }
}";

		const string fixedTest = @"
using UnityEngine;
using Beyond;

namespace Beyond
{
    enum ELogChannel { None }
    enum EColorTag { None }

    static class DLogger
    {
        public static void Log(string message, params object[] args) {}
        public static void LogWarning(UnityEngine.Object context, string message, params object[] args) {}
        public static void LogError(ELogChannel channel, EColorTag color, string message, params object[] args) {}
    }
}

class Camera : MonoBehaviour
{
    void Start()
    {
        int var1 = 123;
        string var2 = ""456"";
        DLogger.Log(""Hello, \""world\""! {0} {1}"", var1, var2);
        DLogger.LogWarning(this, ""Hello, world! {0} {1}"", var1, var2);
        DLogger.LogError(ELogChannel.None, EColorTag.None, ""Hello, world! {0} {1}"", var1, var2);
    }
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithLocation(24, 21),
			ExpectDiagnostic().WithLocation(25, 34),
			ExpectDiagnostic().WithLocation(26, 60),
		};

		await VerifyCSharpDiagnosticAndFixAsync(test, expected, fixedTest);
	}
}
