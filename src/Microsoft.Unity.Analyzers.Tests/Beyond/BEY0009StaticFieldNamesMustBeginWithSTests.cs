#nullable disable

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0009StaticFieldNamesMustBeginWithSAnalyzer"/>.
/// </summary>
public class BEY0009StaticFieldNamesMustBeginWithSTests : BaseCodeFixVerifierTest<BEY0009StaticFieldNamesMustBeginWithSAnalyzer, BEY0009StaticFieldNamesMustBeginWithSCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[
			// Suppress CS0067: warning CS0067: The event 'TypeName.bar' is never used
			"CS0067",
			// Suppress CS0169: warning CS0169: The field 'Foo.Bar' is never used
			"CS0169",
			// Suppress CS0649: warning CS0649: Field 'Foo.Bar' is never assigned to, and will always have its default value null
			"CS0649",
		];
	}

	[Theory]
	[InlineData("const")]
	[InlineData("private const")]
	[InlineData("internal const")]
	[InlineData("protected const")]
	[InlineData("protected internal const")]
	[InlineData("internal readonly")]
	[InlineData("public const")]
	[InlineData("protected readonly")]
	[InlineData("protected internal readonly")]
	[InlineData("public readonly")]
	[InlineData("public")]
	[InlineData("internal")]
	[InlineData("protected internal")]
	[InlineData("public static readonly")]
	[InlineData("internal static readonly")]
	[InlineData("protected internal static readonly")]
	[InlineData("protected static readonly")]
	[InlineData("private static readonly")]
	public async Task TestThatDiagnosticIsNotReportedAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar = """", car = """", Dar = """";
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReported_EventFieldsAsync()
	{
		var testCode = @"using System;
public class TypeName
{
    static event EventHandler bar;
    static event EventHandler Bar;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Theory]
	[InlineData("public static")]
	[InlineData("internal static")]
	[InlineData("protected internal static")]
	public async Task TestThatDiagnosticIsNotReported_PublicStaticFieldsAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar, car, Dar;
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Theory]
	[InlineData("protected static")]
	[InlineData("private static")]
	[InlineData("static")]
	public async Task TestThatDiagnosticIsReported_SingleFieldAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar;
{0}
string s_car;
{0}
string Dar;
{0}
string s_ear;
{0}
string _Far;
{0}
string s__gar;
{0}
string __Har;
{0}
string s___iar;
{0}
string ___Jar;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 8),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(8, 8),
			ExpectDiagnostic().WithArguments("_Far").WithLocation(12, 8),
			ExpectDiagnostic().WithArguments("__Har").WithLocation(16, 8),
			ExpectDiagnostic().WithArguments("___Jar").WithLocation(20, 8),
		};

		var fixedCode = @"public class Foo
{{
{0}
string s_bar;
{0}
string s_car;
{0}
string s_dar;
{0}
string s_ear;
{0}
string s_far;
{0}
string s__gar;
{0}
string s_har;
{0}
string s___iar;
{0}
string s__jar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("protected static")]
	[InlineData("private static")]
	[InlineData("static")]
	public async Task TestThatDiagnosticIsReported_MultipleFieldsAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar, car, Dar;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 8),
			ExpectDiagnostic().WithArguments("car").WithLocation(4, 13),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(4, 18),
		};

		var fixedCode = @"public class Foo
{{
{0}
string s_bar, s_car, s_dar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Fact]
	public async Task TestFieldsWithUnderscoreAsync()
	{
		var testCode = @"public class Foo
{
    private static string _ = ""bar"";
    private static string __ = ""baz"";
    private static string ___ = ""qux"";
    private static string someVar_ = ""bar"";
}";

		var fixedTestCode = @"public class Foo
{
    private static string s_ = ""bar"";
    private static string s_1 = ""baz"";
    private static string s__ = ""qux"";
    private static string s_someVar_ = ""bar"";
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("_").WithLocation(3, 27),
			ExpectDiagnostic().WithArguments("__").WithLocation(4, 27),
			ExpectDiagnostic().WithArguments("___").WithLocation(5, 27),
			ExpectDiagnostic().WithArguments("someVar_").WithLocation(6, 27),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldWithStructAsync()
	{
		var testCode = @"public struct Foo
{
    private static string test1 = ""test1"";
    private static string test2 = ""test2"";
}";

		var fixedTestCode = @"public struct Foo
{
    private static string s_test1 = ""test1"";
    private static string s_test2 = ""test2"";
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("test1").WithLocation(3, 27),
			ExpectDiagnostic().WithArguments("test2").WithLocation(4, 27),
		};
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldWithCodefixRenameConflictAsync()
	{
		var testCode = @"public class Foo
{
    private static string s_test = ""test1"";
    private static string test = ""test2"";
}";

		var fixedTestCode = @"public class Foo
{
    private static string s_test = ""test1"";
    private static string s_test1 = ""test2"";
}";

		var expected = ExpectDiagnostic().WithArguments("test").WithLocation(4, 27);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    static string Bar = ""baz"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
