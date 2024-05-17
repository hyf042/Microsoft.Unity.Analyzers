using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0010NonPublicFieldNamesMustBeginWithMAnalyzer"/>.
/// </summary>
public class BEY0010NonPublicFieldNamesMustBeginWithMTests : BaseCodeFixVerifierTest<BEY0010NonPublicFieldNamesMustBeginWithMAnalyzer, BEY0010NonPublicFieldNamesMustBeginWithMCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[
			// Suppress CS0067: warning CS0067: The event 'TypeName.bar' is never used
			"CS0067",
			// Suppress CS0169: warning CS0169: The field 'Foo.Bar' is never used
			"CS0169",
		];
	}

	[Theory]
	[InlineData("const")]
	[InlineData("public const")]
	[InlineData("private const")]
	[InlineData("internal const")]
	[InlineData("protected const")]
	[InlineData("protected internal const")]
	[InlineData("internal readonly")]
	[InlineData("public readonly")]
	[InlineData("public")]
	[InlineData("internal")]
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
    private event EventHandler bar;
    private event EventHandler Bar;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReported_UnitySerializeField()
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{
    [SerializeField]
    string bar = ""bar"";
    [SerializeField]
    private string Car = ""Car"";
    [SerializeField]
    protected string dar = ""dar"", Ear = ""Ear"";
    [SerializeReference]
    private List<int> keys = default;
    [SerializeReference]
    private List<int> values = default;
}

[System.Serializable]
public class Foo2 : MonoBehaviour
{
    [SerializeField]
    string bar = ""bar"";
    [SerializeField]
    private string Car = ""Car"";
    [SerializeField]
    protected string dar = ""dar"", Ear = ""Ear"";
    [SerializeReference]
    private List<int> keys = default;
    [SerializeReference]
    private List<int> values = default;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}


	[Fact]
	public async Task TestThatDiagnosticIsNotReported_VariablesAsync()
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

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("private readonly")]
	[InlineData("protected readonly")]
	public async Task TestThatDiagnosticIsReported_SingleFieldAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar;
{0}
string m_car;
{0}
string Dar;
{0}
string m_ear;
{0}
string _Far;
{0}
string m__gar;
{0}
string __Har;
{0}
string m___iar;
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
string m_bar;
{0}
string m_car;
{0}
string m_dar;
{0}
string m_ear;
{0}
string m_far;
{0}
string m__gar;
{0}
string m_har;
{0}
string m___iar;
{0}
string m__jar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("private readonly")]
	[InlineData("protected readonly")]
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
string m_bar, m_car, m_dar;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Fact]
	public async Task TestFieldsWithUnderscoreAsync()
	{
		var testCode = @"public class Foo
{
    private string _ = ""bar"";
    private string __ = ""baz"";
    private string ___ = ""qux"";
    private string someVar_ = ""bar"";
}";

		var fixedTestCode = @"public class Foo
{
    private string m_ = ""bar"";
    private string m_1 = ""baz"";
    private string m__ = ""qux"";
    private string m_someVar_ = ""bar"";
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("_").WithLocation(3, 20),
			ExpectDiagnostic().WithArguments("__").WithLocation(4, 20),
			ExpectDiagnostic().WithArguments("___").WithLocation(5, 20),
			ExpectDiagnostic().WithArguments("someVar_").WithLocation(6, 20),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldWithStructAsync()
	{
		var testCode = @"public struct Foo
{
    string test1;
    private string test2;
}";

		var fixedTestCode = @"public struct Foo
{
    string m_test1;
    private string m_test2;
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("test1").WithLocation(3, 12),
			ExpectDiagnostic().WithArguments("test2").WithLocation(4, 20),
		};
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldWithCodefixRenameConflictAsync()
	{
		var testCode = @"public class Foo
{
    private string m_test = ""test1"";
    private string test = ""test2"";
}";

		var fixedTestCode = @"public class Foo
{
    private string m_test = ""test1"";
    private string m_test1 = ""test2"";
}";

		var expected = ExpectDiagnostic().WithArguments("test").WithLocation(4, 20);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    private string Bar = ""baz"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
