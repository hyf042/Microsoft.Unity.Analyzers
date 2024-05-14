/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BeyondStaticFieldNamesMustBeginWithSTests : BaseCodeFixVerifierTest<BeyondStaticFieldNamesMustBeginWithSAnalyzer, BeyondStaticFieldNamesMustBeginWithSCodeFix>
{
	private DiagnosticResult Diagnostic() => ExpectDiagnostic();
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
	public async Task TestThatDiagnosticIsNotReportedForEventFieldsAsync()
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
	[InlineData("protected static")]
	[InlineData("protected internal static")]
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
			Diagnostic().WithArguments("Bar").WithLocation(4, 8),
			Diagnostic().WithArguments("Dar").WithLocation(8, 8),
			Diagnostic().WithArguments("_Far").WithLocation(12, 8),
			Diagnostic().WithArguments("__Har").WithLocation(16, 8),
			Diagnostic().WithArguments("___Jar").WithLocation(20, 8),
		};

		var fixedCode = @"public class Foo
{{
{0}
string s_Bar;
{0}
string s_car;
{0}
string s_Dar;
{0}
string s_ear;
{0}
string s_Far;
{0}
string s__gar;
{0}
string s__Har;
{0}
string s___iar;
{0}
string s___Jar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("public static")]
	[InlineData("internal static")]
	[InlineData("protected static")]
	[InlineData("protected internal static")]
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
			Diagnostic().WithArguments("Bar").WithLocation(4, 8),
			Diagnostic().WithArguments("car").WithLocation(4, 13),
			Diagnostic().WithArguments("Dar").WithLocation(4, 18),
		};

		var fixedCode = @"public class Foo
{{
{0}
string s_Bar, s_car, s_Dar;
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
    private static string s__ = ""baz"";
    private static string s___ = ""qux"";
    private static string s_someVar_ = ""bar"";
}";

		DiagnosticResult[] expected =
		{
			Diagnostic().WithArguments("_").WithLocation(3, 27),
			Diagnostic().WithArguments("__").WithLocation(4, 27),
			Diagnostic().WithArguments("___").WithLocation(5, 27),
			Diagnostic().WithArguments("someVar_").WithLocation(6, 27),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldWithCodefixRenameConflictAsync()
	{
		var testCode = @"public class Foo
{
    public static string s_test = ""test1"";
    public static string test = ""test2"";
}";

		var fixedTestCode = @"public class Foo
{
    public static string s_test = ""test1"";
    public static string s_test1 = ""test2"";
}";

		var expected = Diagnostic().WithArguments("test").WithLocation(4, 26);
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
