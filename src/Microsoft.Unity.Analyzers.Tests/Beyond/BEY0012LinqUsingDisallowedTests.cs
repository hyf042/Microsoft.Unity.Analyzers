using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BEY0012LinqUsingDisallowedTests : BaseCodeFixVerifierTest<BEY0012LinqUsingDisallowedAnalyzer, BEY0012LinqUsingDisallowedCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[
			"CS0105",
			"CS8019",
		];
	}
	protected override string[] PreprocessorSymbols
	{
		get =>
		[
			"SOME_SYMBOL",
		];
	}

	[Fact]
	public async Task TestNotReported_UsingDirectivesAreOnTopInCompilationAsync()
	{
		string usingsInCompilationUnit = @"using System;
using System.IO;
using UnityEngine;

class A
{
}";

		await VerifyCSharpDiagnosticAsync(usingsInCompilationUnit);
	}

	[Fact]
	public async Task TestNotReported_UsingDirectivesAreOnTopInNamespaceAsync()
	{
		string usingsInNamespaceDeclaration = @"namespace Test
{
    using System;
    using System.IO;
    using UnityEngine;

    class A
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(usingsInNamespaceDeclaration);
	}

	[Theory]
	[InlineData("UNITY_EDITOR")]
	[InlineData("BEYOND_DEBUG")]
	public async Task TestNotReported_LinqUsingsWithinAllowedMacrosAsync(string modifiers)
	{
		const string testCode = @"
#if {0} && true
using System.Linq;
using Alias = System.Linq.Expressions;
#endif

namespace TestNamespace
{{
#if true
#elif {0} || false
using System.Linq.Expressions;
#endif
#if !!{0}
using global::System.Linq;
#endif
#if !{0}
#else
using \u0053ystem.Linq;
#endif
class TestObject {{ }}
}}
";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Fact]
	public async Task TestIsReported_UsingDirectivesWithEscapeSequenceAsync()
	{
		string testCode = @"
using @System.Linq;
using Alias = System.Linq.Expressions;
namespace Test
{
    using System.Linq.Expressions;
    using \u0053ystem.Linq;
}";

		string fixedCode = @"namespace Test
{
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(2, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(3, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(6, 5),
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(7, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Theory]
	[InlineData("UNITY_EDITOR")]
	[InlineData("BEYOND_DEBUG")]
	public async Task TestIsReported_LinqUsingsNotWithinAllowedMacrosAsync(string modifiers)
	{
		const string testCode = @"
#if ((!{0}))
using System.Linq;
using Alias = System.Linq.Expressions;
#endif
namespace TestNamespace
{{
#if SOME_SYMBOL
#if (({0}))
#else
using System.Linq.Expressions;
using global::System.Linq;
#endif
#endif
class TestObject {{ }}
}}
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(3, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(4, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(11, 1),
			ExpectDiagnostic().WithArguments("global::System.Linq").WithLocation(12, 1),
		};

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers), expected);
	}
}
