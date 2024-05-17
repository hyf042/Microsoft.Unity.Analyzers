using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0006ElementMustNamedAllCapsAnalyzer"/>.
/// </summary>
public class BEY0006ElementMustNamedAllCapsTests : BaseCodeFixVerifierTest<BEY0006ElementMustNamedAllCapsAnalyzer, BEY0006ElementMustNamedAllCapsCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[ 
			// Suppress CS0169: warning CS0169: The field 'Foo.Bar' is never used
			"CS0169",
			// Suppress CS0649: warning CS0649: Field 'Foo.__HAR' is never assigned to, and will always have its default value 0
			"CS0649",
		];
	}

	[Theory]
	[InlineData("public readonly")]
	[InlineData("private readonly")]
	[InlineData("internal readonly")]
	[InlineData("protected readonly")]
	[InlineData("protected internal readonly")]
	[InlineData("public")]
	[InlineData("private")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	[InlineData("public static")]
	[InlineData("private static")]
	[InlineData("internal static")]
	[InlineData("protected internal static")]
	[InlineData("protected static")]
	public async Task TestThatDiagnosticIsNotReportedAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string Bar = """", car = """", Dar = """";
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	[InlineData("private")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsNotReported_ReadonlyStaticClassAsync(string modifiers)
	{
		var testCode = @"
public class Object {{}}

public class Foo
{{
{0}
readonly static Object Bar = new();
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	[InlineData("private")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsReported_StaticReadonlyStructAsync(string modifiers)
	{
		var testCode = @"
public struct Object {{}}

public class Foo
{{
{0}
readonly static Object Bar = new();
}}";

		var fixedCode = @"
public struct Object {{}}

public class Foo
{{
{0}
readonly static Object BAR = new();
}}";

		var expected = ExpectDiagnostic().WithArguments("Bar").WithLocation(7, 24);

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}


	[Theory]
	[InlineData("")]
	[InlineData("public")]
	[InlineData("private")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsReported_SingleFieldAsync(string modifiers)
	{
		var testCode = @"
public struct Object {{}}

public class Foo
{{
{0}
const int Bar = default;
{0}
const bool car_Dar = default;
{0}
const string _ear = default;
{0}
static readonly float _FAR__GAR = default;
{0}
static readonly double __HAR;
{0}
static readonly Object ___iar___JAR = new();
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(7, 11),
			ExpectDiagnostic().WithArguments("car_Dar").WithLocation(9, 12),
			ExpectDiagnostic().WithArguments("_ear").WithLocation(11, 14),
			ExpectDiagnostic().WithArguments("___iar___JAR").WithLocation(17, 24),
		};

		var fixedCode = @"
public struct Object {{}}

public class Foo
{{
{0}
const int BAR = default;
{0}
const bool CAR_DAR = default;
{0}
const string _EAR = default;
{0}
static readonly float _FAR__GAR = default;
{0}
static readonly double __HAR;
{0}
static readonly Object ___IAR___JAR = new();
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	[InlineData("private")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsReported_MultipleFieldsAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
const string Bar = default, CAR = default, Dar = default;
{0}
static readonly float EAR = default, far = default;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 14),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(4, 44),
			ExpectDiagnostic().WithArguments("far").WithLocation(6, 38),
		};

		var fixedCode = @"public class Foo
{{
{0}
const string BAR = default, CAR = default, DAR = default;
{0}
static readonly float EAR = default, FAR = default;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Fact]
	public async Task TestFieldWithAllUnderscoresAsync()
	{
		var testCode = @"public class Foo
{
    private const string _ = ""bar"";
    private const string __ = ""baz"";
    private const string ___ = ""qux"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestFieldWithCodefixRenameConflictAsync()
	{
		var testCode = @"public class Foo
{
    public const string test = ""test1"";
    public static readonly string Test = ""test2"";
}";

		var fixedTestCode = @"public class Foo
{
    public const string TEST = ""test1"";
    public static readonly string TEST1 = ""test2"";
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Test").WithLocation(3, 25),
			ExpectDiagnostic().WithArguments("Test").WithLocation(4, 35),
		};
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    const string Bar = ""baz"";
    static readonly string Car = ""caz"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
