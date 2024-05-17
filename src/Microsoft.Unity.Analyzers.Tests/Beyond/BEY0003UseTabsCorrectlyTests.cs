using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0003UseTabsCorrectlyAnalyzer"/>.
/// </summary>
public class BEY0003UseTabsCorrectlyTests : BaseCodeFixVerifierTest<BEY0003UseTabsCorrectlyAnalyzer, BEY0003UseTabsCorrectlyCodeFix>
{
	/// <summary>
	/// Verifies that tabs used inside string and char literals are not producing diagnostics.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestValidTabsAsync()
	{
		var testCode =
			"public class Foo\r\n" +
			"{\r\n" +
			"    public const string ValidTestString = \"\tText\";\r\n" +
			"    public const char ValidTestChar = '\t';\r\n" +
			"}\r\n";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that tabs used inside disabled code are not producing diagnostics.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDisabledCodeAsync()
	{
		var testCode =
			"public class Foo\r\n" +
			"{\r\n" +
			"#if false\r\n" +
			"\tpublic const string ValidTestString = \"Text\";\r\n" +
			"\tpublic const char ValidTestChar = 'c';\r\n" +
			"#endif\r\n" +
			"}\r\n";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestInvalidTabsAsync()
	{
		var testCode =
			"using\tSystem.Diagnostics;\r\n" +
			"\r\n" +
			"public\tclass\tFoo\r\n" +
			"{\r\n" +
			"\tpublic void Bar()\r\n" +
			"\t{\r\n" +
			"\t  \t// Comment\r\n" +
			"\t \tDebug.Indent();\r\n" +
			"   \t}\r\n" +
			"}\r\n";

		var fixedTestCode = @"using   System.Diagnostics;

public  class   Foo
{
    public void Bar()
    {
        // Comment
        Debug.Indent();
    }
}
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithLocation(1, 6),
			ExpectDiagnostic().WithLocation(3, 7),
			ExpectDiagnostic().WithLocation(3, 13),
			ExpectDiagnostic().WithLocation(5, 1),
			ExpectDiagnostic().WithLocation(6, 1),
			ExpectDiagnostic().WithLocation(7, 1),
			ExpectDiagnostic().WithLocation(8, 1),
			ExpectDiagnostic().WithLocation(9, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestInvalidTabsInDocumentationCommentsAsync()
	{
		var testCode =
			"\t///\t<summary>\r\n" +
			"\t/// foo\tbar\r\n" +
			"\t///\t</summary>\r\n" +
			"\tpublic class Foo\r\n" +
			"\t{\r\n" +
			"\t \t/// <MyElement>\tValue </MyElement>\r\n" +
			"\t\t/**\t \t<MyElement> Value </MyElement>\t*/\r\n" +
			"\t}\r\n";

		var fixedTestCode = @"    /// <summary>
    /// foo bar
    /// </summary>
    public class Foo
    {
        /// <MyElement> Value </MyElement>
        /**     <MyElement> Value </MyElement>  */
    }
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithLocation(1, 1),
			ExpectDiagnostic().WithLocation(1, 5),
			ExpectDiagnostic().WithLocation(2, 1),
			ExpectDiagnostic().WithLocation(2, 9),
			ExpectDiagnostic().WithLocation(3, 1),
			ExpectDiagnostic().WithLocation(3, 5),
			ExpectDiagnostic().WithLocation(4, 1),
			ExpectDiagnostic().WithLocation(5, 1),
			ExpectDiagnostic().WithLocation(6, 1),
			ExpectDiagnostic().WithLocation(6, 19),
			ExpectDiagnostic().WithLocation(7, 1),
			ExpectDiagnostic().WithLocation(7, 6),
			ExpectDiagnostic().WithLocation(7, 39),
			ExpectDiagnostic().WithLocation(8, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestInvalidTabsInCommentsAsync()
	{
		var testCode =
			"\tpublic class Foo\r\n" +
			"\t{\r\n" +
			"\t\tpublic void Bar()\r\n" +
			"\t\t{\r\n" +
			"\t\t \t//\tComment\t\t1\r\n" +
			"            ////\tCommented Code\t\t1\r\n" +
			"\t  \t\t// Comment 2\r\n" +
			"\t\t}\r\n" +
			"\t}\r\n";

		var fixedTestCode =
			"    public class Foo\r\n" +
			"    {\r\n" +
			"        public void Bar()\r\n" +
			"        {\r\n" +
			"            //  Comment     1\r\n" +
			"            ////\tCommented Code\t\t1\r\n" +
			"            // Comment 2\r\n" +
			"        }\r\n" +
			"    }\r\n";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithLocation(1, 1),
			ExpectDiagnostic().WithLocation(2, 1),
			ExpectDiagnostic().WithLocation(3, 1),
			ExpectDiagnostic().WithLocation(4, 1),
			ExpectDiagnostic().WithLocation(5, 1),
			ExpectDiagnostic().WithLocation(5, 7),
			ExpectDiagnostic().WithLocation(5, 15),
			ExpectDiagnostic().WithLocation(7, 1),
			ExpectDiagnostic().WithLocation(8, 1),
			ExpectDiagnostic().WithLocation(9, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestInvalidTabsInMultiLineCommentsAsync()
	{
		var testCode =
			"\tpublic class Foo\r\n" +
			"\t{\r\n" +
			"\t\tpublic void Bar()\r\n" +
			"\t\t{\r\n" +
			"\t\t \t/*\r\n" +
			"\t\t\tComment\t\t1\r\n" +
			"\t  \t\tComment 2\r\n" +
			"  \t\t\t*/\r\n" +
			"\t\t}\r\n" +
			"\t}\r\n";

		var fixedTestCode = @"    public class Foo
    {
        public void Bar()
        {
            /*
            Comment     1
            Comment 2
            */
        }
    }
";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithLocation(1, 1),
			ExpectDiagnostic().WithLocation(2, 1),
			ExpectDiagnostic().WithLocation(3, 1),
			ExpectDiagnostic().WithLocation(4, 1),
			ExpectDiagnostic().WithLocation(5, 1),
			ExpectDiagnostic().WithLocation(6, 1),
			ExpectDiagnostic().WithLocation(6, 11),
			ExpectDiagnostic().WithLocation(7, 1),
			ExpectDiagnostic().WithLocation(8, 1),
			ExpectDiagnostic().WithLocation(9, 1),
			ExpectDiagnostic().WithLocation(10, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}
}
