/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BEY0012LinqUsingDisallowedTests : BaseCodeFixVerifierTest<BEY0012LinqUsingDisallowedAnalyzer, BEY0012LinqUsingDisallowedCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[
			// Suppress CS0105: warning CS0105: The using directive for 'System.Linq' appeared previously in this namespace
			"CS0105",
			// Suppress CS8019: Mismatch between number of diagnostics returned, expected "0" actual "1"
			"CS8019",
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

	[Fact]
	public async Task TestIsReported_UsingDirectivesWithEscapeSequenceAsync()
	{
		string testCode = @"namespace Test
{
    using @System.Linq;
    using System.Linq.Expressions;
    using \u0053ystem.Linq;
}";

		string fixedCode = @"namespace Test
{
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(3, 5),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(4, 5),
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(5, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestIsReported_LinqUsings()
	{
		const string testCode = @"
using System.Linq;
using Alias = System.Linq.Expressions;

namespace TestNamespace
{
using System.Linq.Expressions;
using global::System.Linq;
class TestObject { }
}
";
		const string fixedCode = @"
namespace TestNamespace
{
class TestObject { }
}
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("System.Linq").WithLocation(2, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(3, 1),
			ExpectDiagnostic().WithArguments("System.Linq.Expressions").WithLocation(7, 1),
			ExpectDiagnostic().WithArguments("global::System.Linq").WithLocation(8, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}
}
