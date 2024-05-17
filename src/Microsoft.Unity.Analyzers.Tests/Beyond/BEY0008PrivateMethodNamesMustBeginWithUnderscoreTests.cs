using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0008PrivateMethodNamesMustBeginWithUnderscoreAnalyzer"/>.
/// </summary>
public class BEY0008PrivateMethodNamesMustBeginWithUnderscoreTests : BaseCodeFixVerifierTest<BEY0008PrivateMethodNamesMustBeginWithUnderscoreAnalyzer, BEY0008PrivateMethodNamesMustBeginWithUnderscoreCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[ 
			// Suppress CS0067: warning CS8321: The local function 'LocalFunction' is declared but never used
			"CS8321",
		];
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsNotReported_UnityMessage(string modifiers)
	{
		const string test = @"
using UnityEngine;

class Camera : MonoBehaviour
{{
{0}
void Awake() {{}}
{0}
void Start() {{}}
{0}
void OnDestroy() {{}}
{0}
void FixedUpdate() {{}}
{0}
void OnApplicationPause(bool pause) {{}}
}}

class StateMachine : ScriptableObject
{{
{0}
void Awake() {{}}
{0}
void OnDestroy() {{}}
{0}
void OnDisable() {{}}
{0}
void OnEnable() {{}}
{0}
void OnValidate() {{}}
{0}
void Reset() {{}}
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(test, modifiers));
	}

	[Theory]
	[InlineData("public")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsNotReported_NonPrivateMethod(string modifiers)
	{
		const string test = @"public abstract class BaseBehaviour
{{
    {0} abstract void Bar();
}}

public class Foo : BaseBehaviour
{{
{0}
override void Bar() {{}}
{0}
void Car() {{}}
{0}
virtual void Dar() {{}}
{0}
static void Ear() {{}}
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(test, modifiers));
	}


	[Fact]
	public async Task TestThatDiagnosticIsNotReported_LocalFunctionAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        void LocalFunction() { }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsReported_NormalClass(string modifiers)
	{
		const string test = @"public class Foo
{{
{0}
void Bar() {{}}
{0}
static void Car() {{}}
{0}
void Dar(int a, int b) {{}}
{0}
static void Ear(string message) {{}}
{0}
void _2DTest() {{}}
}}";

		const string fixCode = @"public class Foo
{{
{0}
void _Bar() {{}}
{0}
static void _Car() {{}}
{0}
void _Dar(int a, int b) {{}}
{0}
static void _Ear(string message) {{}}
{0}
void _2DTest() {{}}
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 6),
			ExpectDiagnostic().WithArguments("Car").WithLocation(6, 13),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(8, 6),
			ExpectDiagnostic().WithArguments("Ear").WithLocation(10, 13),
		};

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(test, modifiers), expected, string.Format(fixCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsReported_NormalStruct(string modifiers)
	{
		const string test = @"public struct Foo
{{
{0}
void Bar() {{}}
{0}
static void Car() {{}}
{0}
void Dar(int a, int b) {{}}
{0}
static void Ear(string message) {{}}
}}";

		const string fixCode = @"public struct Foo
{{
{0}
void _Bar() {{}}
{0}
static void _Car() {{}}
{0}
void _Dar(int a, int b) {{}}
{0}
static void _Ear(string message) {{}}
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 6),
			ExpectDiagnostic().WithArguments("Car").WithLocation(6, 13),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(8, 6),
			ExpectDiagnostic().WithArguments("Ear").WithLocation(10, 13),
		};

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(test, modifiers), expected, string.Format(fixCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsReported_UnityClass(string modifiers)
	{
		const string test = @"
using UnityEngine;

public class Foo : MonoBehaviour
{{
{0}
void Update() {{}}
{0}
static void Car() {{}}
{0}
void Dar(int a, int b) {{}}
{0}
static void Ear(string message) {{}}
}}";

		const string fixCode = @"
using UnityEngine;

public class Foo : MonoBehaviour
{{
{0}
void Update() {{}}
{0}
static void _Car() {{}}
{0}
void _Dar(int a, int b) {{}}
{0}
static void _Ear(string message) {{}}
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Car").WithLocation(9, 13),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(11, 6),
			ExpectDiagnostic().WithArguments("Ear").WithLocation(13, 13),
		};

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(test, modifiers), expected, string.Format(fixCode, modifiers));
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    private void TestMethod() {}
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
