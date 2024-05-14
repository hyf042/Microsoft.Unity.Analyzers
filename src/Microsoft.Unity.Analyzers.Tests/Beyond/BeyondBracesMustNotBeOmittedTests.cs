/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BeyondBracesMustNotBeOmittedAnalyzer"/>.
/// </summary>
public class BeyondBracesMustNotBeOmittedTests : BaseCodeFixVerifierTest<BeyondBracesMustNotBeOmittedAnalyzer, BeyondBracesMustNotBeOmittedCodeFix>
{
	/// <summary>
	/// Gets the statements that will be used in the theory test cases.
	/// </summary>
	/// <value>
	/// The statements that will be used in the theory test cases.
	/// </value>
	public static IEnumerable<object[]> TestStatements
	{
		get
		{
			yield return new[] { "if (i == 0)" };
			yield return new[] { "while (i == 0)" };
			yield return new[] { "for (var j = 0; j < i; j++)" };
			yield return new[] { "foreach (var j in new[] { 1, 2, 3 })" };
			yield return new[] { "lock (this)" };
			yield return new[] { "using (this)" };
			yield return new[] { "fixed (byte* ptr = new byte[10])" };
		}
	}

	protected override bool IgnoreLineEndingDifferences => true;
	protected override bool AllowUnsafe => true;

	private DiagnosticResult Diagnostic() => ExpectDiagnostic();

	/// <summary>
	/// Verifies that a statement followed by a block without braces will produce a warning.
	/// </summary>
	/// <param name="statementText">The source code for the first part of a compound statement whose child can be
	/// either a statement block or a single statement.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Theory]
	[MemberData(nameof(TestStatements))]
	public async Task TestStatementWithoutBracesAsync(string statementText)
	{
		var expected = Diagnostic().WithLocation(7, 13);
		await VerifyCSharpDiagnosticAsync(this.GenerateTestStatement(statementText), expected);
	}

	/// <summary>
	/// Verifies that a <c>do</c> statement followed by a block without braces will produce a warning, and the
	/// code fix for this warning results in valid code.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDoStatementAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        do
            Debug.Assert(true);
        while (false);
    }
}";
		var fixedCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        do
        {
            Debug.Assert(true);
        }
        while (false);
    }
}";

		var expected = Diagnostic().WithLocation(7, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	/// <summary>
	/// Verifies that a statement followed by a block with braces will produce no diagnostics results.
	/// </summary>
	/// <param name="statementText">The source code for the first part of a compound statement whose child can be
	/// either a statement block or a single statement.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Theory]
	[MemberData(nameof(TestStatements))]
	public async Task TestStatementWithBracesAsync(string statementText)
	{
		await VerifyCSharpDiagnosticAsync(this.GenerateFixedTestStatement(statementText));
	}

	/// <summary>
	/// Verifies that an if / else statement followed by a block without braces will produce a warning.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfElseStatementWithoutBracesAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
            Debug.Assert(true);
        else
            Debug.Assert(false);
    }
}";

		var expected = new[]
		{
			Diagnostic().WithLocation(7, 13),
			Diagnostic().WithLocation(9, 13),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	/// <summary>
	/// Verifies that an if statement followed by a block with braces will produce no diagnostics results.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfElseStatementWithBracesAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else
        {
            Debug.Assert(false);
        }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that an if statement followed by a else if, followed by an else statement, all blocks with braces
	/// will produce no diagnostics results.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfElseIfElseStatementWithBracesAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else if (i == 1)
        {
            Debug.Assert(false);
        }
        else
        {
            Debug.Assert(true);
        }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that nested if statements followed by a block without braces will produce warnings.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestMultipleIfStatementsWithoutBracesAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0) if (i == 0) Debug.Assert(true);
    }
}";

		var expected = new[]
		{
			Diagnostic().WithLocation(6, 21),
			Diagnostic().WithLocation(6, 33),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	/// <summary>
	/// Verifies that the codefix provider will work properly for an if .. else statement.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestCodeFixProviderForIfElseStatementAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
            Debug.Assert(true);
        else
            Debug.Assert(false);
    }
}";

		var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            Debug.Assert(true);
        }
        else
        {
            Debug.Assert(false);
        }
    }
}";

		var expected = new[]
		{
			Diagnostic().WithLocation(7, 13),
			Diagnostic().WithLocation(9, 13),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	/// <summary>
	/// Verifies that the codefix provider will work properly handle non-whitespace trivia.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestCodeFixProviderWithNonWhitespaceTriviaAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
#pragma warning restore
        if (i == 0)
            Debug.Assert(true);
    }
}";

		var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
#pragma warning restore
        if (i == 0)
        {
            Debug.Assert(true);
        }
    }
}";

		var expected = Diagnostic().WithLocation(8, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	/// <summary>
	/// Verifies that the codefix provider will work properly handle multiple cases of missing braces.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestCodeFixProviderWithMultipleNestingsAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0) if (i == 0) Debug.Assert(true);
    }
}";

		var fixedTestCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (i == 0)
        {
            if (i == 0)
            {
                Debug.Assert(true);
            }
        }
    }
}";

		DiagnosticResult[] expected =
		{
			Diagnostic().WithLocation(6, 21),
			Diagnostic().WithLocation(6, 33),
		};
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestCodeFixProviderWithMultiLineChildStatement()
	{
		var testCode = @"
public class Foo
{
    public int Bar(int value)
    {
        if (value > 10)
            return
                value;

        if (value < 10)
            if (value > 5)
                return value;

        return 0;
    }
}";

		var fixedTestCode = @"
public class Foo
{
    public int Bar(int value)
    {
        if (value > 10)
        {
            return
                value;
        }

        if (value < 10)
        {
            if (value > 5)
            {
                return value;
            }
        }

        return 0;
    }
}";

		DiagnosticResult[] expected =
		{
			Diagnostic().WithLocation(7, 13),
			Diagnostic().WithLocation(11, 13),
			Diagnostic().WithLocation(12, 17),
		};
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);

	}

	/// <summary>
	/// Verifies that the code fix provider will not perform a code fix when statement contains a preprocessor directive.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestCodeFixWithPreprocessorDirectivesAsync()
	{
		var testCode = @"using System.Diagnostics;
public class Foo
{
    public void Bar(int i)
    {
        if (true)
#if !DEBUG
            Debug.WriteLine(""Foobar"");
#endif
        Debug.WriteLine(""Foobar 2"");
    }
}";

		var expected = Diagnostic().WithLocation(8, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, testCode);
	}

	/// <summary>
	/// This is a regression test for https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/2184.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestMultipleUsingStatementsAsync()
	{
		var testCode = @"using System;
public class Foo
{
    public void Bar(int i)
    {
        using (default(IDisposable))
        using (default(IDisposable))
        {
        }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that the codefix provider will work properly for a statement.
	/// </summary>
	/// <param name="statementText">The source code for the first part of a compound statement whose child can be
	/// either a statement block or a single statement.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Theory]
	[MemberData(nameof(TestStatements))]
	public async Task TestCodeFixForStatementAsync(string statementText)
	{
		var expected = Diagnostic().WithLocation(7, 13);
		await VerifyCSharpDiagnosticAndFixAsync(
			this.GenerateTestStatement(statementText), expected, this.GenerateFixedTestStatement(statementText));
	}

	private string GenerateTestStatement(string statementText)
	{
		var testCodeFormat = @"using System.Diagnostics;
public class Foo : System.IDisposable
{
    public unsafe void Bar(int i)
    {
        #STATEMENT#
            Debug.Assert(true);
    }

    public void Dispose() {}
}";
		return testCodeFormat.Replace("#STATEMENT#", statementText);
	}

	private string GenerateFixedTestStatement(string statementText)
	{
		var fixedTestCodeFormat = @"using System.Diagnostics;
public class Foo : System.IDisposable
{
    public unsafe void Bar(int i)
    {
        #STATEMENT#
        {
            Debug.Assert(true);
        }
    }

    public void Dispose() {}
}";
		return fixedTestCodeFormat.Replace("#STATEMENT#", statementText);
	}
}
