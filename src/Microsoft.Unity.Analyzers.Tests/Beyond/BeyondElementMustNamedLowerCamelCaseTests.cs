/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BeyondElementMustNamedLowerCamelCaseTests : BaseCodeFixVerifierTest<BeyondElementMustNamedLowerCamelCaseAnalyzer, BeyondElementMustNamedLowerCamelCaseCodeFix>
{
	protected override bool ExpectErrorAsDiagnosticResult => true;

	private DiagnosticResult Diagnostic() => ExpectDiagnostic();
	protected override string[] DisabledDiagnostics
	{
		get =>
		[ 
			// Suppress CS0067: warning CS0067: The event 'TypeName.bar' is never used
			"CS0067",
			// Suppress CS0168: warning CS0168: The variable 'Variable' is declared but never used
			"CS0168",
			// Suppress CS0169: warning CS0169: The field 'Foo.Bar' is never used
			"CS0169",
			// Suppress CS0219: The variable 'Constant' is assigned but its value is never used
			"CS0219",
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
	[InlineData("public static")]
	[InlineData("internal static")]
	[InlineData("protected internal static")]
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
    event EventHandler car;
    event EventHandler Car;
}";

		var expected = new[]
		{
			Diagnostic().WithArguments("Bar").WithLocation(5, 31),
			Diagnostic().WithArguments("Car").WithLocation(7, 24),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReportedForParametersAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string bar, string Car)
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReportedForVariablesAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        const string bar = nameof(bar);
        const string Bar = nameof(Bar);
        string car = nameof(car);
        string Car = nameof(Car);
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// This test ensures the implementation of <see cref="SA1306FieldNamesMustBeginWithLowerCaseLetter"/> is
	/// correct with respect to the documented behavior for parameters and local variables (including local
	/// constants).
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestThatDiagnosticIsNotReportedForParametersAndLocalsAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string Parameter1, string parameter2)
    {
        const int Constant = 1;
        const int constant = 1;
        int Variable;
        int variable;
        int Variable1, Variable2;
        int variable1, variable2;
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Theory]
	[InlineData("")]
	[InlineData("readonly")]
	[InlineData("private")]
	[InlineData("private readonly")]
	[InlineData("static")]
	[InlineData("private static")]
	[InlineData("protected static")]
	public async Task TestThatDiagnosticIsReported_SingleFieldAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar;
{0}
string car;
{0}
string Dar;
{0}
string _ear;
{0}
string _Far;
{0}
string __gar;
{0}
string __Har;
{0}
string ___iar;
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
string bar;
{0}
string car;
{0}
string dar;
{0}
string _ear;
{0}
string _far;
{0}
string __gar;
{0}
string __har;
{0}
string ___iar;
{0}
string ___jar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Theory]
	[InlineData("")]
	[InlineData("readonly")]
	[InlineData("private")]
	[InlineData("private readonly")]
	[InlineData("static")]
	[InlineData("private static")]
	[InlineData("protected static")]
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
			Diagnostic().WithArguments("Dar").WithLocation(4, 18),
		};

		var fixedCode = @"public class Foo
{{
{0}
string bar, car, dar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Fact]
	public async Task TestFieldStartingWithLetterAsync()
	{
		var testCode = @"public class Foo
{
    public string bar = ""baz"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestFieldWithAllUnderscoresAsync()
	{
		var testCode = @"public class Foo
{
    private string _ = ""bar"";
    private string __ = ""baz"";
    private string ___ = ""qux"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestFieldWithTrailingUnderscoreAsync()
	{
		var testCode = @"public class Foo
{
    private string someVar_ = ""bar"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestFieldWithCodefixRenameConflictAsync()
	{
		var testCode = @"public class Foo
{
    public string test = ""test1"";
    public string Test = ""test2"";
}";

		var fixedTestCode = @"public class Foo
{
    public string test = ""test1"";
    public string test1 = ""test2"";
}";

		var expected = Diagnostic().WithArguments("Test").WithLocation(4, 19);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    string Bar = ""baz"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
