using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BEY0013UnityDebugLogDisallowedTests : BaseCodeFixVerifierTest<BEY0013UnityDebugLogDisallowedAnalyzer, BEY0013UnityDebugLogDisallowedCodeFix>
{
	[Fact]
	public async Task TestNotReported()
	{
		const string test = @"
static class Debug
{
    public static void Log(string message, params object[] args) {}
    public static void LogWarning(string message, params object[] args) {}
    public static void LogError(string message, params object[] args) {}
}

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

static class DLogger
{
    public static void Log(string message, params object[] args) {}
    public static void LogWarning(string message, params object[] args) {}
    public static void LogError(string message, params object[] args) {}
}

class Camera : MonoBehaviour
{
    void Start()
    {
        int var1 = 123;
        string var2 = ""456"";
        Debug.Log(""Hello, world!"");
        Debug.LogWarning(""Hello, world! {var1} {var2}"");
        Debug.LogError($""Hello, world! {var1} {var2}"");
        Debug.Assert(false);
        // Test a comment before
        Debug.LogFormat(""Hello, world!"") /* tail comment */;
        // Test a comment after
        Debug.LogWarningFormat(""Hello, world! {0}"", var1);
        Debug.LogErrorFormat(""Hello, world! {0} {1}"", var1, var2);
    }
}
";

		const string fixedCode = @"
using UnityEngine;

static class DLogger
{
    public static void Log(string message, params object[] args) {}
    public static void LogWarning(string message, params object[] args) {}
    public static void LogError(string message, params object[] args) {}
}

class Camera : MonoBehaviour
{
    void Start()
    {
        int var1 = 123;
        string var2 = ""456"";
        DLogger.Log(""Hello, world!"");
        DLogger.LogWarning(""Hello, world! {var1} {var2}"");
        DLogger.LogError(""Hello, world! {0} {1}"", var1, var2);
        Debug.Assert(false);
        // Test a comment before
        DLogger.Log(""Hello, world!"") /* tail comment */;
        // Test a comment after
        DLogger.LogWarning(""Hello, world! {0}"", var1);
        DLogger.LogError(""Hello, world! {0} {1}"", var1, var2);
    }
}
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.Log(object)").WithLocation(17, 9),
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.LogWarning(object)").WithLocation(18, 9),
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.LogError(object)").WithLocation(19, 9),
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.LogFormat(string, params object[])").WithLocation(22, 9),
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.LogWarningFormat(string, params object[])").WithLocation(24, 9),
			ExpectDiagnostic().WithArguments("UnityEngine.Debug.LogErrorFormat(string, params object[])").WithLocation(25, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(test, expected, fixedCode);
	}
}
