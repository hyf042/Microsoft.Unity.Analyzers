#nullable disable

using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0002BracesForMultiLineStatementsMustNotShareLineAnalyzer"/>.
/// </summary>
public class BEY0002BracesForMultiLineStatementsMustNotShareLineTests : BaseCodeFixVerifierTest<BEY0002BracesForMultiLineStatementsMustNotShareLineAnalyzer, BEY0002BracesForMultiLineStatementsMustNotShareLineCodeFix>
{
	protected override bool IgnoreLineEndingDifferences => true;
	protected override bool AllowUnsafe => true;
	protected override bool ExpectErrorAsDiagnosticResult => true;

	protected override string[] DisabledDiagnostics
	{
		get =>
		[ 
			// Suppress CS8019: Unnecessary using directive
			"CS8019",
			// Suppress CS0162: Unreachable code detected
			"CS0162",
			// Suppress CS1058: A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a System.Runtime.CompilerServices.RuntimeWrappedException.
			"CS1058",
		];
	}

	/// <summary>
	/// Verifies that a complex multiple fix scenario is handled correctly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task VerifyFixAllAsync()
	{
		var testCode = @"using System;
public class TestClass
{
    public static void Sample()
    {
        try {
            if (false) {
                return;
            } else if (false) {
                return;
            } else {
                return;
            }
        } catch (Exception) {
        } catch {
        } finally {
        }
    }
}
";

		var fixedTestCode = @"using System;
public class TestClass
{
    public static void Sample()
    {
        try
        {
            if (false)
            {
                return;
            }
            else if (false)
            {
                return;
            }
            else
            {
                return;
            }
        }
        catch (Exception)
        {
        }
        catch
        {
        }
        finally
        {
        }
    }
}
";

		DiagnosticResult[] expectedDiagnostics =
		{
			ExpectDiagnostic().WithLocation(6, 13),
			ExpectDiagnostic().WithLocation(7, 24),
			ExpectDiagnostic().WithLocation(9, 13),
			ExpectDiagnostic().WithLocation(9, 31),
			ExpectDiagnostic().WithLocation(11, 13),
			ExpectDiagnostic().WithLocation(11, 20),
			ExpectDiagnostic().WithLocation(14, 9),
			ExpectDiagnostic().WithLocation(14, 29),
			ExpectDiagnostic().WithLocation(15, 9),
			ExpectDiagnostic().WithLocation(15, 17),
			ExpectDiagnostic().WithLocation(16, 9),
			ExpectDiagnostic().WithLocation(16, 19),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#region Array Initializers

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid array initializers defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestArrayInitializersValidAsync()
	{
		var testCode = @"
using System;

public class TestClass
{
    private int[] array1 = { };

    private int[] array2 = { 0, 1 };

    private int[] array3 = new[] { 0, 1 };

    private int[] array3b = new int[] { 0, 1 };

    private int[] array4 =
    {
    };

    private int[] array5 =
    {
        0,
        1,
    };

    private int[] array6 = new[]
    {
        0,
    };

    public void TestMethod()
    {
        int[] array7 = { };

        int[] array8 = { 0, 1 };

        var array9 = new[] { 0, 1 };

        int[] array10 =
        {
        };

        int[] array11 =
        {
            0,
            1,
        };

        var array12 = new[]
        {
            0,
        };

        Console.WriteLine(new[] { 0, 1 });
        Console.WriteLine(new int[] { 0, 1 });
        Console.WriteLine(new int[] { });
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid implicit array initializer definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestImplicitArrayInitializersInvalidAsync()
	{
		var testCode = @"
public class TestClass
{
    private int[] invalidArray1 =
        { };

    private int[] invalidArray2 =
        { 0, 1 };

    private int[] invalidArray3 =
        { 0, 1
        };

    private int[] invalidArray4 =
        {
            0, 1 };

    private int[] invalidArray5 = new[]
        { 0, 1 };

    private int[] invalidArray6 = new[]
        { 0, 1
        };

    private int[] invalidArray7 = new[]
        {
            0, 1 };

    public void TestMethod()
    {
        int[] invalidArray8 =
            { };

        int[] invalidArray9 =
            { 0, 1 };

        int[] invalidArray10 =
            { 0, 1
            };

        int[] invalidArray11 =
            {
                0, 1 };

        var invalidArray12 = new[]
            { 0, 1 };

        var invalidArray13 = new[]
            { 0, 1
            };

        var invalidArray14 = new[]
            {
                0, 1 };
    }
}";

		var fixedTestCode = @"
public class TestClass
{
    private int[] invalidArray1 =
        {
        };

    private int[] invalidArray2 =
        {
            0, 1
        };

    private int[] invalidArray3 =
        {
            0, 1
        };

    private int[] invalidArray4 =
        {
            0, 1
        };

    private int[] invalidArray5 = new[]
        {
            0, 1
        };

    private int[] invalidArray6 = new[]
        {
            0, 1
        };

    private int[] invalidArray7 = new[]
        {
            0, 1
        };

    public void TestMethod()
    {
        int[] invalidArray8 =
            {
            };

        int[] invalidArray9 =
            {
                0, 1
            };

        int[] invalidArray10 =
            {
                0, 1
            };

        int[] invalidArray11 =
            {
                0, 1
            };

        var invalidArray12 = new[]
            {
                0, 1
            };

        var invalidArray13 = new[]
            {
                0, 1
            };

        var invalidArray14 = new[]
            {
                0, 1
            };
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			ExpectDiagnostic().WithLocation(5, 9),
			ExpectDiagnostic().WithLocation(5, 11),
			ExpectDiagnostic().WithLocation(8, 9),
			ExpectDiagnostic().WithLocation(8, 16),
			ExpectDiagnostic().WithLocation(11, 9),
			ExpectDiagnostic().WithLocation(16, 18),
			ExpectDiagnostic().WithLocation(19, 9),
			ExpectDiagnostic().WithLocation(19, 16),
			ExpectDiagnostic().WithLocation(22, 9),
			ExpectDiagnostic().WithLocation(27, 18),
			ExpectDiagnostic().WithLocation(32, 13),
			ExpectDiagnostic().WithLocation(32, 15),
			ExpectDiagnostic().WithLocation(35, 13),
			ExpectDiagnostic().WithLocation(35, 20),
			ExpectDiagnostic().WithLocation(38, 13),
			ExpectDiagnostic().WithLocation(43, 22),
			ExpectDiagnostic().WithLocation(46, 13),
			ExpectDiagnostic().WithLocation(46, 20),
			ExpectDiagnostic().WithLocation(49, 13),
			ExpectDiagnostic().WithLocation(54, 22),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid array initializer definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestArrayInitializersInvalidAsync()
	{
		var testCode = @"
public class TestClass
{
    private int[] invalidArray5 = new int[]
        { 0, 1 };

    private int[] invalidArray6 = new int[]
        { 0, 1
        };

    private int[] invalidArray7 = new int[]
        {
            0, 1 };

    public void TestMethod()
    {
        var invalidArray12 = new int[]
            { 0, 1 };

        var invalidArray13 = new int[]
            { 0, 1
            };

        var invalidArray14 = new int[]
            {
                0, 1 };
    }
}";

		var fixedTestCode = @"
public class TestClass
{
    private int[] invalidArray5 = new int[]
        {
            0, 1
        };

    private int[] invalidArray6 = new int[]
        {
            0, 1
        };

    private int[] invalidArray7 = new int[]
        {
            0, 1
        };

    public void TestMethod()
    {
        var invalidArray12 = new int[]
            {
                0, 1
            };

        var invalidArray13 = new int[]
            {
                0, 1
            };

        var invalidArray14 = new int[]
            {
                0, 1
            };
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
				ExpectDiagnostic().WithLocation(5, 9),
				ExpectDiagnostic().WithLocation(5, 16),
				ExpectDiagnostic().WithLocation(8, 9),
				ExpectDiagnostic().WithLocation(13, 18),
				ExpectDiagnostic().WithLocation(18, 13),
				ExpectDiagnostic().WithLocation(18, 20),
				ExpectDiagnostic().WithLocation(21, 13),
				ExpectDiagnostic().WithLocation(26, 22),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that a multi-dimensional array initialization produces the expected diagnostics.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task VerifyMultidimensionalArrayInitializationAsync()
	{
		var testCode = @"
public class TestClass
{
    private static readonly float[,] TestMatrix1 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };

    private static readonly float[,] TestMatrix2 =
        new float[,]
        {   { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };

    private static readonly float[,] TestMatrix3 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 } };

    private static readonly float[,] TestMatrix4 =
        new float[,]
        {
            { 0, 0, 1, 1 }, { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };
}
";

		var fixedTestCode = @"
public class TestClass
{
    private static readonly float[,] TestMatrix1 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };

    private static readonly float[,] TestMatrix2 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };

    private static readonly float[,] TestMatrix3 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };

    private static readonly float[,] TestMatrix4 =
        new float[,]
        {
            { 0, 0, 1, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 }
        };
}
";

		DiagnosticResult[] expectedDiagnostics =
		{
				ExpectDiagnostic().WithLocation(14, 9),
				ExpectDiagnostic().WithLocation(24, 28),
				ExpectDiagnostic().WithLocation(29, 29),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that a jagged array initialization produces the expected diagnostics.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task VerifyJaggedArrayInitializationAsync()
	{
		var testCode = @"
public class TestClass
{
    private static readonly int[][] TestMatrix1 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };

    private static readonly int[][] TestMatrix2 =
        new int[][]
        {   new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };

    private static readonly int[][] TestMatrix3 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 } };

    private static readonly int[][] TestMatrix4 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 }, new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };
}
";

		var fixedTestCode = @"
public class TestClass
{
    private static readonly int[][] TestMatrix1 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };

    private static readonly int[][] TestMatrix2 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };

    private static readonly int[][] TestMatrix3 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 },
            new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };

    private static readonly int[][] TestMatrix4 =
        new int[][]
        {
            new[] { 0, 0, 1, 1 }, new[] { 1, 1, 1, 0 },
            new[] { 0, 1, 0, 0 }
        };
}
";

		DiagnosticResult[] expectedDiagnostics =
		{
				ExpectDiagnostic().WithLocation(14, 9),
				ExpectDiagnostic().WithLocation(24, 34),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Blocks

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid block defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestBlockValidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    public void Bar()
    {
        // valid block #1
        {
        }

        // valid block #2
        {
            Debug.Indent();
        }

        // valid block #3
        { }

        // valid block #4
        { Debug.Indent(); }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies diagnostics and codefixes for all invalid blocks.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestBlockInvalidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    public void Bar()
    {
        // invalid block #1
        { Debug.Indent();
        }

        // invalid block #2
        {
            Debug.Indent(); }
    }
}";

		var fixedTestCode = @"using System.Diagnostics;

public class Foo
{
    public void Bar()
    {
        // invalid block #1
        {
            Debug.Indent();
        }

        // invalid block #2
        {
            Debug.Indent();
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
				ExpectDiagnostic().WithLocation(8, 9),
				ExpectDiagnostic().WithLocation(13, 29),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Constructors

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid constructors defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestConstructorValidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Valid constructor #1
    public Foo()
    {
    }

    // Valid constructor #2
    public Foo(bool a)
    {
        Debug.Indent();
    }

    // Valid constructor #3
    public Foo(byte a) { }

    // Valid constructor #4
    public Foo(short a) { Debug.Indent(); }

    // Valid constructor #5
    public Foo(ushort a) 
    { Debug.Indent(); }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies diagnostics and codefixes for all invalid constructor definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestConstructorInvalidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid constructor #1
    public Foo() {
    }

    // Invalid constructor #2
    public Foo(bool a) {
        Debug.Indent();
    }

    // Invalid constructor #3
    public Foo(byte a) {
        Debug.Indent(); }

    // Invalid constructor #4
    public Foo(short a) { Debug.Indent();
    }

    // Invalid constructor #5
    public Foo(ushort a)
    {
        Debug.Indent(); }

    // Invalid constructor #6
    public Foo(int a)
    { Debug.Indent();
    }
}";

		var fixedTestCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid constructor #1
    public Foo()
    {
    }

    // Invalid constructor #2
    public Foo(bool a)
    {
        Debug.Indent();
    }

    // Invalid constructor #3
    public Foo(byte a)
    {
        Debug.Indent();
    }

    // Invalid constructor #4
    public Foo(short a)
    {
        Debug.Indent();
    }

    // Invalid constructor #5
    public Foo(ushort a)
    {
        Debug.Indent();
    }

    // Invalid constructor #6
    public Foo(int a)
    {
        Debug.Indent();
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid constructor #1
			ExpectDiagnostic().WithLocation(6, 18),

			// Invalid constructor #2
			ExpectDiagnostic().WithLocation(10, 24),

			// Invalid constructor #3
			ExpectDiagnostic().WithLocation(15, 24),
			ExpectDiagnostic().WithLocation(16, 25),

			// Invalid constructor #4
			ExpectDiagnostic().WithLocation(19, 25),

			// Invalid constructor #5
			ExpectDiagnostic().WithLocation(25, 25),

			// Invalid constructor #6
			ExpectDiagnostic().WithLocation(29, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Delegates

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid delegates defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDelegateValidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    private delegate void MyDelegate();

    private void TestMethod(MyDelegate d)
    {
    }

    private void Bar()
    {
        // Valid delegate #1
        MyDelegate item1 = delegate { };
        
        // Valid delegate #2
        MyDelegate item2 = delegate { Debug.Indent(); };

        // Valid delegate #3
        MyDelegate item3 = delegate
        {
        };

        // Valid delegate #4
        MyDelegate item4 = delegate
        {
            Debug.Indent();
        };

        // Valid delegate #5
        MyDelegate item7 = delegate 
        { Debug.Indent(); };

        // Valid delegate #6
        this.TestMethod(delegate { });

        // Valid delegate #7
        this.TestMethod(delegate 
        { 
        });

        // Valid delegate #8
        this.TestMethod(delegate { Debug.Indent(); });

        // Valid delegate #9
        this.TestMethod(delegate 
        { 
            Debug.Indent(); 
        });

        // Valid delegate #10
        this.TestMethod(delegate 
        { Debug.Indent(); });
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies diagnostics and codefixes for all invalid delegate definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDelegateInvalidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    private delegate void MyDelegate();

    private void TestMethod(MyDelegate d)
    {
    }

    private void Bar()
    {
        // Invalid delegate #1
        MyDelegate item1 = delegate {
        };
        
        // Invalid delegate #2
        MyDelegate item2 = delegate {
            Debug.Indent(); 
        };

        // Invalid delegate #3
        MyDelegate item3 = delegate {
            Debug.Indent(); };

        // Invalid delegate #4
        MyDelegate item4 = delegate { Debug.Indent();
        };

        // Invalid delegate #5
        MyDelegate item5 = delegate
        {
            Debug.Indent(); };

        // Invalid delegate #6
        MyDelegate item6 = delegate
        { Debug.Indent();
        };

        // Invalid delegate #7
        this.TestMethod(delegate {
        });

        // Invalid delegate #8
        this.TestMethod(delegate {
            Debug.Indent();
        });

        // Invalid delegate #9
        this.TestMethod(delegate {
            Debug.Indent(); });

        // Invalid delegate #10
        this.TestMethod(delegate { Debug.Indent();
        });

        // Invalid delegate #11
        this.TestMethod(delegate
        {
            Debug.Indent(); });

        // Invalid delegate #12
        this.TestMethod(delegate
        { Debug.Indent();
        });
    }
}";

		var fixedTestCode = @"using System.Diagnostics;

public class Foo
{
    private delegate void MyDelegate();

    private void TestMethod(MyDelegate d)
    {
    }

    private void Bar()
    {
        // Invalid delegate #1
        MyDelegate item1 = delegate
        {
        };
        
        // Invalid delegate #2
        MyDelegate item2 = delegate
        {
            Debug.Indent(); 
        };

        // Invalid delegate #3
        MyDelegate item3 = delegate
        {
            Debug.Indent();
        };

        // Invalid delegate #4
        MyDelegate item4 = delegate
        {
            Debug.Indent();
        };

        // Invalid delegate #5
        MyDelegate item5 = delegate
        {
            Debug.Indent();
        };

        // Invalid delegate #6
        MyDelegate item6 = delegate
        {
            Debug.Indent();
        };

        // Invalid delegate #7
        this.TestMethod(delegate
        {
        });

        // Invalid delegate #8
        this.TestMethod(delegate
        {
            Debug.Indent();
        });

        // Invalid delegate #9
        this.TestMethod(delegate
        {
            Debug.Indent();
        });

        // Invalid delegate #10
        this.TestMethod(delegate
        {
            Debug.Indent();
        });

        // Invalid delegate #11
        this.TestMethod(delegate
        {
            Debug.Indent();
        });

        // Invalid delegate #12
        this.TestMethod(delegate
        {
            Debug.Indent();
        });
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid delegate #1
			ExpectDiagnostic().WithLocation(14, 37),

			// Invalid delegate #2
			ExpectDiagnostic().WithLocation(18, 37),

			// Invalid delegate #3
			ExpectDiagnostic().WithLocation(23, 37),
			ExpectDiagnostic().WithLocation(24, 29),

			// Invalid delegate #4
			ExpectDiagnostic().WithLocation(27, 37),

			// Invalid delegate #5
			ExpectDiagnostic().WithLocation(33, 29),

			// Invalid delegate #6
			ExpectDiagnostic().WithLocation(37, 9),

			// Invalid delegate #7
			ExpectDiagnostic().WithLocation(41, 34),

			// Invalid delegate #8
			ExpectDiagnostic().WithLocation(45, 34),

			// Invalid delegate #9
			ExpectDiagnostic().WithLocation(50, 34),
			ExpectDiagnostic().WithLocation(51, 29),

			// Invalid delegate #10
			ExpectDiagnostic().WithLocation(54, 34),

			// Invalid delegate #11
			ExpectDiagnostic().WithLocation(60, 29),

			// Invalid delegate #12
			ExpectDiagnostic().WithLocation(64, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Destructors

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid destructors defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDestructorValidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Valid destructor #1
    public class TestClass1
    {
        ~TestClass1()
        {
        }
    }

    // Valid destructor #2
    public class TestClass2
    {
        ~TestClass2()
        {
            Debug.Indent();
        }
    }

    // Valid destructor #3
    public class TestClass3
    {
        ~TestClass3() { }
    }

    // Valid destructor #4
    public class TestClass4
    {
        ~TestClass4() { Debug.Indent(); }
    }

    // Valid destructor #5
    public class TestClass5
    {
        ~TestClass5() 
        { Debug.Indent(); }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics and codefixes for all invalid destructor definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDestructorInvalidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid destructor #1
    public class TestClass1
    {
        ~TestClass1() {
        }
    }

    // Invalid destructor #2
    public class TestClass2
    {
        ~TestClass2() {
            Debug.Indent();
        }
    }

    // Invalid destructor #3
    public class TestClass3
    {
        ~TestClass3() {
            Debug.Indent(); }
    }

    // Invalid destructor #4
    public class TestClass4
    {
        ~TestClass4() { Debug.Indent();
        }
    }

    // Invalid destructor #5
    public class TestClass5
    {
        ~TestClass5()
        {
            Debug.Indent(); }
    }

    // Invalid destructor #6
    public class TestClass6
    {
        ~TestClass6()
        { Debug.Indent();
        }
    }
}";

		var fixedTestCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid destructor #1
    public class TestClass1
    {
        ~TestClass1()
        {
        }
    }

    // Invalid destructor #2
    public class TestClass2
    {
        ~TestClass2()
        {
            Debug.Indent();
        }
    }

    // Invalid destructor #3
    public class TestClass3
    {
        ~TestClass3()
        {
            Debug.Indent();
        }
    }

    // Invalid destructor #4
    public class TestClass4
    {
        ~TestClass4()
        {
            Debug.Indent();
        }
    }

    // Invalid destructor #5
    public class TestClass5
    {
        ~TestClass5()
        {
            Debug.Indent();
        }
    }

    // Invalid destructor #6
    public class TestClass6
    {
        ~TestClass6()
        {
            Debug.Indent();
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid destructor #1
			ExpectDiagnostic().WithLocation(8, 23),

			// Invalid destructor #2
			ExpectDiagnostic().WithLocation(15, 23),

			// Invalid destructor #3
			ExpectDiagnostic().WithLocation(23, 23),
			ExpectDiagnostic().WithLocation(24, 29),

			// Invalid destructor #4
			ExpectDiagnostic().WithLocation(30, 23),

			// Invalid destructor #5
			ExpectDiagnostic().WithLocation(39, 29),

			// Invalid destructor #6
			ExpectDiagnostic().WithLocation(46, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region DoWhiles

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid do ... while statements defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDoWhileValidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid do ... while #1
        do
        {
        }
        while (x == 0);

        // Valid do ... while #2
        do
        {
            x = 1;
        }
        while (x == 0);

        // Valid do ... while #3
        do { } while (x == 0);

        // Valid do ... while #4
        do { x = 1; } while (x == 0);

        // Valid do ... while #5
        do 
        { x = 1; } while (x == 0);
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid do ... while statement definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDoWhileInvalidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid do ... while #1
        do
        {
        } while (x == 0);

        // Invalid do ... while #2
        do {
        }
        while (x == 0);

        // Invalid do ... while #3
        do {
        } while (x == 0);

        // Valid do ... while #4
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #5
        do
        {
            x = 1; }
        while (x == 0);

        // Invalid do ... while #6
        do
        {
            x = 1; } while (x == 0);

        // Invalid do ... while #7
        do
        { x = 1;
        }
        while (x == 0);

        // Invalid do ... while #8
        do
        { x = 1;
        } while (x == 0);

        // Invalid do ... while #9
        do {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #10
        do {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #11
        do {
            x = 1; }
        while (x == 0);

        // Invalid do ... while #12
        do {
            x = 1; } while (x == 0);

        // Invalid do ... while #13
        do { x = 1;
        }
        while (x == 0);

        // Invalid do ... while #14
        do { x = 1;
        } while (x == 0);
    }
}";

		var fixedTestCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid do ... while #1
        do
        {
        } while (x == 0);

        // Invalid do ... while #2
        do
        {
        }
        while (x == 0);

        // Invalid do ... while #3
        do
        {
        } while (x == 0);

        // Valid do ... while #4
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #5
        do
        {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #6
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #7
        do
        {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #8
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #9
        do
        {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #10
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #11
        do
        {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #12
        do
        {
            x = 1;
        } while (x == 0);

        // Invalid do ... while #13
        do
        {
            x = 1;
        }
        while (x == 0);

        // Invalid do ... while #14
        do
        {
            x = 1;
        } while (x == 0);
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid do ... while #2
			ExpectDiagnostic().WithLocation(13, 12),

			// Invalid do ... while #3
			ExpectDiagnostic().WithLocation(18, 12),

			// Invalid do ... while #5
			ExpectDiagnostic().WithLocation(30, 20),

			// Invalid do ... while #6
			ExpectDiagnostic().WithLocation(36, 20),

			// Invalid do ... while #7
			ExpectDiagnostic().WithLocation(40, 9),

			// Invalid do ... while #8
			ExpectDiagnostic().WithLocation(46, 9),

			// Invalid do ... while #9
			ExpectDiagnostic().WithLocation(50, 12),

			// Invalid do ... while #10
			ExpectDiagnostic().WithLocation(56, 12),

			// Invalid do ... while #11
			ExpectDiagnostic().WithLocation(61, 12),
			ExpectDiagnostic().WithLocation(62, 20),

			// Invalid do ... while #12
			ExpectDiagnostic().WithLocation(66, 12),
			ExpectDiagnostic().WithLocation(67, 20),

			// Invalid do ... while #13
			ExpectDiagnostic().WithLocation(70, 12),

			// Invalid do ... while #14
			ExpectDiagnostic().WithLocation(75, 12),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that no diagnostics are reported for the do/while loop when the <see langword="while"/>
	/// expression is on the same line as the closing brace and the setting is enabled.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestDoWhileOnClosingBraceWithAllowSettingAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        do
        {
            x = 1;
        } while (x == 0);
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for the invalid <see langword="while"/> loop that
	/// is on the same line as the closing brace which is not part of a <c>do/while</c> loop. This
	/// ensures that the <c>allowDoWhileOnClosingBrace</c> setting is only applicable to a <c>do/while</c>
	/// loop and will not mistakenly allow a plain <see langword="while"/> loop after any arbitrary
	/// closing brace.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestJustWhileLoopOnClosingBraceWithAllowDoWhileOnClosingBraceSettingAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        while (x == 0)
        {
            x = 1;
        } while (x == 0)
        {
            x = 1;
        }
    }
}";

		var fixedCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        while (x == 0)
        {
            x = 1;
        }
        while (x == 0)
        {
            x = 1;
        }
    }
}";

		await VerifyCSharpFixAsync(testCode, fixedCode);
	}

	/// <summary>
	/// Verifies that no diagnostics are reported for the do/while loop when the <see langword="while"/>
	/// expression is on the same line as the closing brace and the setting is allowed.
	/// </summary>
	/// <remarks>
	/// <para>The "Invalid do ... while #6" code in the <see cref="TestDoWhileInvalidAsync"/> unit test
	/// should account for the proper fix when the <c>allowDoWhileOnClosingBrace</c> is <see langword="false"/>,
	/// which is the default.</para>
	/// </remarks>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestFixDoWhileOnClosingBraceWithAllowSettingAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        do
        {
            x = 1; } while (x == 0);
    }
}";

		var fixedTestCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        do
        {
            x = 1;
        } while (x == 0);
    }
}";

		await VerifyCSharpFixAsync(testCode, fixedTestCode);
	}

	#endregion

	#region Enums

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid enums defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestEnumValidAsync()
	{
		var testCode = @"public class Foo
{
    public enum ValidEnum1
    {
    }

    public enum ValidEnum2
    {
        Test
    }

    public enum ValidEnum3 { }

    public enum ValidEnum4 { Test }

    public enum ValidEnum5
    { Test }  
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid enum definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestEnumInvalidAsync()
	{
		var testCode = @"public class Foo
{
    public enum InvalidEnum1 {
    }

    public enum InvalidEnum2 {
        Test
    }

    public enum InvalidEnum3 {
        Test }

    public enum InvalidEnum4 { Test
    }

    public enum InvalidEnum5
    {
        Test }

    public enum InvalidEnum6
    { Test
    }
}";

		var fixedTestCode = @"public class Foo
{
    public enum InvalidEnum1
    {
    }

    public enum InvalidEnum2
    {
        Test
    }

    public enum InvalidEnum3
    {
        Test
    }

    public enum InvalidEnum4
    {
        Test
    }

    public enum InvalidEnum5
    {
        Test
    }

    public enum InvalidEnum6
    {
        Test
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// InvalidEnum1
			ExpectDiagnostic().WithLocation(3, 30),

			// InvalidEnum2
			ExpectDiagnostic().WithLocation(6, 30),

			// InvalidEnum3
			ExpectDiagnostic().WithLocation(10, 30),
			ExpectDiagnostic().WithLocation(11, 14),

			// InvalidEnum4
			ExpectDiagnostic().WithLocation(13, 30),

			// InvalidEnum5
			ExpectDiagnostic().WithLocation(18, 14),

			// InvalidEnum6
			ExpectDiagnostic().WithLocation(21, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Events
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid events defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestEventValidAsync()
	{
		var testCode = @"using System;

public class Foo
{
    private EventHandler test;

    // Valid event #1
    public event EventHandler Event1
    {
        add { this.test += value; }
        remove { this.test -= value; }
    }

    // Valid event #2
    public event EventHandler Event2
    {
        add 
        { 
            this.test += value; 
        }

        remove 
        { 
            this.test -= value; 
        }
    }

    // Valid event #3
    public event EventHandler Event3
    {
        add { this.test += value; }
        
        remove 
        { 
        }
    }

    // Valid event #4
    public event EventHandler Event4
    {
        add 
        { 
            this.test += value; 
        }

        remove { }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid event definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestEventInvalidAsync()
	{
		var testCode = @"using System;

public class Foo
{
    private EventHandler test;

    // Invalid event #1
    public event EventHandler Event1
    {
        add {
            this.test += value;
        }

        remove {
            this.test -= value;
        }
    }

    // Invalid event #2
    public event EventHandler Event2
    {
        add {
            this.test += value; }

        remove {
            this.test -= value; }
    }

    // Invalid event #3
    public event EventHandler Event3
    {
        add { this.test += value;
        }

        remove { this.test -= value;
        }
    }

    // Invalid event #4
    public event EventHandler Event4
    {
        add 
        {
            this.test += value; }

        remove 
        {
            this.test -= value; }
    }

    // Invalid event #5
    public event EventHandler Event5
    {
        add
        { this.test += value;
        }

        remove
        { this.test -= value;
        }
    }

    // Invalid event #6
    public event EventHandler Event6
    {
        add
        { this.test += value; }

        remove
        { this.test -= value; }
    }
}";

		var fixedTestCode = @"using System;

public class Foo
{
    private EventHandler test;

    // Invalid event #1
    public event EventHandler Event1
    {
        add
        {
            this.test += value;
        }

        remove
        {
            this.test -= value;
        }
    }

    // Invalid event #2
    public event EventHandler Event2
    {
        add
        {
            this.test += value;
        }

        remove
        {
            this.test -= value;
        }
    }

    // Invalid event #3
    public event EventHandler Event3
    {
        add
        {
            this.test += value;
        }

        remove
        {
            this.test -= value;
        }
    }

    // Invalid event #4
    public event EventHandler Event4
    {
        add 
        {
            this.test += value;
        }

        remove 
        {
            this.test -= value;
        }
    }

    // Invalid event #5
    public event EventHandler Event5
    {
        add
        {
            this.test += value;
        }

        remove
        {
            this.test -= value;
        }
    }

    // Invalid event #6
    public event EventHandler Event6
    {
        add { this.test += value; }

        remove { this.test -= value; }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid event #1
			ExpectDiagnostic().WithLocation(10, 13),
			ExpectDiagnostic().WithLocation(14, 16),

			// Invalid event #2
			ExpectDiagnostic().WithLocation(22, 13),
			ExpectDiagnostic().WithLocation(23, 33),
			ExpectDiagnostic().WithLocation(25, 16),
			ExpectDiagnostic().WithLocation(26, 33),

			// Invalid event #3
			ExpectDiagnostic().WithLocation(32, 13),
			ExpectDiagnostic().WithLocation(35, 16),

			// Invalid event #4
			ExpectDiagnostic().WithLocation(44, 33),
			ExpectDiagnostic().WithLocation(48, 33),

			// Invalid event #5
			ExpectDiagnostic().WithLocation(55, 9),
			ExpectDiagnostic().WithLocation(59, 9),

			// Invalid event #6 (Only report once for accessor statement on a single line)
			ExpectDiagnostic().WithLocation(67, 9),
			ExpectDiagnostic().WithLocation(70, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Ifs
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid if statements defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfValidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid if #1
        if (x == 0)
        {
        }

        // Valid if #2
        if (x == 0)
        {
            x = 1;
        }

        // Valid if #3
        if (x == 0) { }

        // Valid if #4
        if (x == 0) { x = 1; }

        // Valid if #5
        if (x == 0) 
        { x = 1; }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid if ... else statements defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfElseValidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid if ... else #1
        if (x == 0)
        {
        }
        else
        {
        }

        // Valid if ... else #2
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }

        // Valid if ... else #3
        if (x == 0)
        {
        }
        else { }

        // Valid if ... else #4
        if (x == 0) 
        {
        }
        else { x = 1; }

        // Valid if ... else #5
        if (x == 0) 
        {
        }
        else 
        { x = 1; }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid if statement definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfInvalidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid if #1
        if (x == 0) {
        }

        // Invalid if #2
        if (x == 0) {
            x = 1;
        }

        // Invalid if #3
        if (x == 0) {
            x = 1; }

        // Invalid if #4
        if (x == 0) { x = 1;
        }

        // Invalid if #5
        if (x == 0)
        {
            x = 1; }

        // Invalid if #6
        if (x == 0)
        { x = 1;
        }
    }
}";

		var fixedTestCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid if #1
        if (x == 0)
        {
        }

        // Invalid if #2
        if (x == 0)
        {
            x = 1;
        }

        // Invalid if #3
        if (x == 0)
        {
            x = 1;
        }

        // Invalid if #4
        if (x == 0)
        {
            x = 1;
        }

        // Invalid if #5
        if (x == 0)
        {
            x = 1;
        }

        // Invalid if #6
        if (x == 0)
        {
            x = 1;
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid if #1
			ExpectDiagnostic().WithLocation(8, 21),

			// Invalid if #2
			ExpectDiagnostic().WithLocation(12, 21),

			// Invalid if #3
			ExpectDiagnostic().WithLocation(17, 21),
			ExpectDiagnostic().WithLocation(18, 20),

			// Invalid if #4
			ExpectDiagnostic().WithLocation(21, 21),

			// Invalid if #5
			ExpectDiagnostic().WithLocation(27, 20),

			// Invalid if #6
			ExpectDiagnostic().WithLocation(31, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid if ... else statement definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIfElseInvalidAsync()
	{
		var testCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid if ... else #1
        if (x == 0)
        {
        }
        else {
        }

        // Invalid if ... else #2
        if (x == 0)
        {
        }
        else {
            x = 1;
        }

        // Invalid if ... else #3
        if (x == 0)
        {
        }
        else {
            x = 1; }

        // Invalid if ... else #4
        if (x == 0)
        {
        }
        else { x = 1;
        }

        // Invalid if ... else #5
        if (x == 0)
        {
        }
        else
        {
            x = 1; }

        // Invalid if ... else #6
        if (x == 0)
        {
        }
        else
        { x = 1;
        }
    }
}";

		var fixedTestCode = @"public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid if ... else #1
        if (x == 0)
        {
        }
        else
        {
        }

        // Invalid if ... else #2
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }

        // Invalid if ... else #3
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }

        // Invalid if ... else #4
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }

        // Invalid if ... else #5
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }

        // Invalid if ... else #6
        if (x == 0)
        {
        }
        else
        {
            x = 1;
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid if ... else #1
			ExpectDiagnostic().WithLocation(11, 14),

			// Invalid if ... else #2
			ExpectDiagnostic().WithLocation(18, 14),

			// Invalid if ... else #3
			ExpectDiagnostic().WithLocation(26, 14),
			ExpectDiagnostic().WithLocation(27, 20),

			// Invalid if ... else #4
			ExpectDiagnostic().WithLocation(33, 14),

			// Invalid if ... else #5
			ExpectDiagnostic().WithLocation(42, 20),

			// Invalid if ... else #6
			ExpectDiagnostic().WithLocation(49, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Indexers
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid indexers defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIndexerValidAsync()
	{
		var testCode = @"public class Foo
{
    private bool test;

    // Valid indexer #1
    public bool this[byte index]
    {
        get { return this.test; }
        set { this.test = value; }
    }

    // Valid indexer #2
    public bool this[sbyte index]
    {
        get 
        { 
            return this.test; 
        }

        set 
        { 
            this.test = value; 
        }
    }

    // Valid indexer #3
    public bool this[short index]
    {
        get { return this.test; }

        set 
        { 
            this.test = value; 
        }
    }

    // Valid indexer #4
    public bool this[ushort index]
    {
        get 
        { 
            return this.test; 
        }

        set { this.test = value; }
    }

    // Valid indexer #5
    public bool this[int index] { get { return this.test; }  set { this.test = value; } }

    // Valid indexer #6
    public bool this[uint index]
    { get { return this.test; }  set { this.test = value; } }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid indexer definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestIndexerInvalidAsync()
	{
		var testCode = @"public class Foo
{
    private bool test;

    // Invalid indexer #1
    public bool this[byte index]
    {
        get {
            return this.test;
        }

        set {
            this.test = value;
        }
    }

    // Invalid indexer #2
    public bool this[sbyte index]
    {
        get {
            return this.test; }

        set {
            this.test = value; }
    }

    // Invalid indexer #3
    public bool this[short index]
    {
        get { return this.test;
        }

        set { this.test = value;
        }
    }

    // Invalid indexer #4
    public bool this[ushort index]
    {
        get
        {
            return this.test; }

        set
        {
            this.test = value; }
    }

    // Invalid indexer #5
    public bool this[int index]
    {
        get
        { return this.test;
        }

        set
        { this.test = value;
        }
    }

    // Invalid indexer #6
    public bool this[uint index]
    {
        get
        { return this.test; }

        set
        { this.test = value; }
    }

    // Invalid indexer #7
    public bool this[long index] {
        get { return this.test; }
    }

    // Invalid indexer #8
    public bool this[ulong index] {
        get { return this.test; } }

    // Invalid indexer #9
    public bool this[bool index] { get { return this.test; }
    }

    // Invalid indexer #10
    public bool this[char index]
    {
        get { return this.test; } }

    // Invalid indexer #11
    public bool this[string index]
    { get { return this.test; }
    }
}";

		var fixedTestCode = @"public class Foo
{
    private bool test;

    // Invalid indexer #1
    public bool this[byte index]
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid indexer #2
    public bool this[sbyte index]
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid indexer #3
    public bool this[short index]
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid indexer #4
    public bool this[ushort index]
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid indexer #5
    public bool this[int index]
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid indexer #6
    public bool this[uint index]
    {
        get { return this.test; }

        set { this.test = value; }
    }

    // Invalid indexer #7
    public bool this[long index]
    {
        get { return this.test; }
    }

    // Invalid indexer #8
    public bool this[ulong index]
    {
        get { return this.test; }
    }

    // Invalid indexer #9
    public bool this[bool index]
    {
        get { return this.test; }
    }

    // Invalid indexer #10
    public bool this[char index]
    {
        get { return this.test; }
    }

    // Invalid indexer #11
    public bool this[string index]
    {
        get { return this.test; }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid indexer #1
			ExpectDiagnostic().WithLocation(8, 13),
			ExpectDiagnostic().WithLocation(12, 13),

			// Invalid indexer #2
			ExpectDiagnostic().WithLocation(20, 13),
			ExpectDiagnostic().WithLocation(21, 31),
			ExpectDiagnostic().WithLocation(23, 13),
			ExpectDiagnostic().WithLocation(24, 32),

			// Invalid indexer #3
			ExpectDiagnostic().WithLocation(30, 13),
			ExpectDiagnostic().WithLocation(33, 13),

			// Invalid indexer #4
			ExpectDiagnostic().WithLocation(42, 31),
			ExpectDiagnostic().WithLocation(46, 32),

			// Invalid indexer #5
			ExpectDiagnostic().WithLocation(53, 9),
			ExpectDiagnostic().WithLocation(57, 9),

			// Invalid indexer #6 (Only report once for accessor statements on a single line)
			ExpectDiagnostic().WithLocation(65, 9),
			ExpectDiagnostic().WithLocation(68, 9),

			// Invalid indexer #7
			ExpectDiagnostic().WithLocation(72, 34),

			// Invalid indexer #8
			ExpectDiagnostic().WithLocation(77, 35),
			ExpectDiagnostic().WithLocation(78, 35),

			// Invalid indexer #9
			ExpectDiagnostic().WithLocation(81, 34),

			// Invalid indexer #10
			ExpectDiagnostic().WithLocation(87, 35),

			// Invalid indexer #11
			ExpectDiagnostic().WithLocation(91, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Interfaces
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid interfaces defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestInterfaceValidAsync()
	{
		var testCode = @"public class Foo
{
    public interface ValidInterface1
    {
    }

    public interface ValidInterface2
    {
        void Bar();
    }

    public interface ValidInterface3 { }

    public interface ValidInterface4 { void Bar(); }

    public interface ValidInterface5
    { void Bar(); }  
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid interface definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestInterfaceInvalidAsync()
	{
		var testCode = @"public class Foo
{
    public interface InvalidInterface1 {
    }

    public interface InvalidInterface2 {
        void Bar();
    }

    public interface InvalidInterface3 {
        void Bar(); }

    public interface InvalidInterface4 { void Bar();
    }

    public interface InvalidInterface5
    {
        void Bar(); }

    public interface InvalidInterface6
    { void Bar();
    }
}";

		var fixedTestCode = @"public class Foo
{
    public interface InvalidInterface1
    {
    }

    public interface InvalidInterface2
    {
        void Bar();
    }

    public interface InvalidInterface3
    {
        void Bar();
    }

    public interface InvalidInterface4
    {
        void Bar();
    }

    public interface InvalidInterface5
    {
        void Bar();
    }

    public interface InvalidInterface6
    {
        void Bar();
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// InvalidInterface1
			ExpectDiagnostic().WithLocation(3, 40),

			// InvalidInterface2
			ExpectDiagnostic().WithLocation(6, 40),

			// InvalidInterface3
			ExpectDiagnostic().WithLocation(10, 40),
			ExpectDiagnostic().WithLocation(11, 21),

			// InvalidInterface4
			ExpectDiagnostic().WithLocation(13, 40),

			// InvalidInterface5
			ExpectDiagnostic().WithLocation(18, 21),

			// InvalidInterface6
			ExpectDiagnostic().WithLocation(21, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region LambdaExpressions
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid lambda expressions defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestLambdaExpressionValidAsync()
	{
		var testCode = @"using System;
using System.Diagnostics;

public class Foo
{
    private void TestMethod(Action action)
    {
    }

    private void Bar()
    {
        // Valid lambda expression #1
        Action item1 = () => { };
        
        // Valid lambda expression #2
        Action item2 = () => { Debug.Indent(); };

        // Valid lambda expression #3
        Action item3 = () =>
        {
        };

        // Valid lambda expression #4
        Action item4 = () =>
        {
            Debug.Indent();
        };

        // Valid lambda expression #5
        Action item5 = () =>
        { Debug.Indent(); };

        // Valid lambda expression #6
        this.TestMethod(() => { });

        // Valid lambda expression #7
        this.TestMethod(() =>
        { 
        });

        // Valid lambda expression #8
        this.TestMethod(() => { Debug.Indent(); });

        // Valid lambda expression #9
        this.TestMethod(() =>
        { 
            Debug.Indent(); 
        });

        // Valid lambda expression #10
        this.TestMethod(() =>
        { Debug.Indent(); });
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid lambda expression definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestLambdaExpressionInvalidAsync()
	{
		var testCode = @"using System;
using System.Diagnostics;
public class Foo
{
    private void TestMethod(Action action)
    {
    }

    private void Bar()
    {
        // Invalid lambda expression #1
        Action item1 = () => {
        };
        
        // Invalid lambda expression #2
        Action item2 = () => {
            Debug.Indent();
        };

        // Invalid lambda expression #3
        Action item3 = () => {
            Debug.Indent(); };

        // Invalid lambda expression #4
        Action item4 = () => { Debug.Indent();
        };

        // Invalid lambda expression #5
        Action item5 = () =>
        {
            Debug.Indent(); };

        // Invalid lambda expression #6
        Action item6 = () =>
        { Debug.Indent();
        };

        // Invalid lambda expression #7
        this.TestMethod(() => {
        });

        // Invalid lambda expression #8
        this.TestMethod(() => {
            Debug.Indent();
        });

        // Invalid lambda expression #9
        this.TestMethod(() => {
            Debug.Indent(); });

        // Invalid lambda expression #10
        this.TestMethod(() => { Debug.Indent();
        });

        // Invalid lambda expression #11
        this.TestMethod(() =>
        {
            Debug.Indent(); });

        // Invalid lambda expression #12
        this.TestMethod(() =>
        { Debug.Indent();
        });
    }
}";

		var fixedTestCode = @"using System;
using System.Diagnostics;
public class Foo
{
    private void TestMethod(Action action)
    {
    }

    private void Bar()
    {
        // Invalid lambda expression #1
        Action item1 = () =>
        {
        };
        
        // Invalid lambda expression #2
        Action item2 = () =>
        {
            Debug.Indent();
        };

        // Invalid lambda expression #3
        Action item3 = () =>
        {
            Debug.Indent();
        };

        // Invalid lambda expression #4
        Action item4 = () =>
        {
            Debug.Indent();
        };

        // Invalid lambda expression #5
        Action item5 = () =>
        {
            Debug.Indent();
        };

        // Invalid lambda expression #6
        Action item6 = () =>
        {
            Debug.Indent();
        };

        // Invalid lambda expression #7
        this.TestMethod(() =>
        {
        });

        // Invalid lambda expression #8
        this.TestMethod(() =>
        {
            Debug.Indent();
        });

        // Invalid lambda expression #9
        this.TestMethod(() =>
        {
            Debug.Indent();
        });

        // Invalid lambda expression #10
        this.TestMethod(() =>
        {
            Debug.Indent();
        });

        // Invalid lambda expression #11
        this.TestMethod(() =>
        {
            Debug.Indent();
        });

        // Invalid lambda expression #12
        this.TestMethod(() =>
        {
            Debug.Indent();
        });
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid lambda expression #1
			ExpectDiagnostic().WithLocation(12, 30),

			// Invalid lambda expression #2
			ExpectDiagnostic().WithLocation(16, 30),

			// Invalid lambda expression #3
			ExpectDiagnostic().WithLocation(21, 30),
			ExpectDiagnostic().WithLocation(22, 29),

			// Invalid lambda expression #4
			ExpectDiagnostic().WithLocation(25, 30),

			// Invalid lambda expression #5
			ExpectDiagnostic().WithLocation(31, 29),

			// Invalid lambda expression #6
			ExpectDiagnostic().WithLocation(35, 9),

			// Invalid lambda expression #7
			ExpectDiagnostic().WithLocation(39, 31),

			// Invalid lambda expression #8
			ExpectDiagnostic().WithLocation(43, 31),

			// Invalid lambda expression #9
			ExpectDiagnostic().WithLocation(48, 31),
			ExpectDiagnostic().WithLocation(49, 29),

			// Invalid lambda expression #10
			ExpectDiagnostic().WithLocation(52, 31),

			// Invalid lambda expression #11
			ExpectDiagnostic().WithLocation(58, 29),

			// Invalid lambda expression #12
			ExpectDiagnostic().WithLocation(62, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Methods
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid methods defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestMethodValidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Valid method #1
    public void Method1()
    {
    }

    // Valid method #2
    public void Method2()
    {
        Debug.Indent();
    }

    // Valid method #3
    public void Method3() { }

    // Valid method #4
    public void Method4() { Debug.Indent(); }

    // Valid method #5
    public void Method5() 
    { Debug.Indent(); }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid method definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestMethodInvalidAsync()
	{
		var testCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid method #1
    public void Method1() {
    }

    // Invalid method #2
    public void Method2() {
        Debug.Indent();
    }

    // Invalid method #3
    public void Method3() {
        Debug.Indent(); }

    // Invalid method #4
    public void Method4() { Debug.Indent();
    }

    // Invalid method #5
    public void Method5()
    {
        Debug.Indent(); }

    // Invalid method #6
    public void Method6()
    { Debug.Indent();
    }
}";

		var fixedTestCode = @"using System.Diagnostics;

public class Foo
{
    // Invalid method #1
    public void Method1()
    {
    }

    // Invalid method #2
    public void Method2()
    {
        Debug.Indent();
    }

    // Invalid method #3
    public void Method3()
    {
        Debug.Indent();
    }

    // Invalid method #4
    public void Method4()
    {
        Debug.Indent();
    }

    // Invalid method #5
    public void Method5()
    {
        Debug.Indent();
    }

    // Invalid method #6
    public void Method6()
    {
        Debug.Indent();
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid method #1
			ExpectDiagnostic().WithLocation(6, 27),

			// Invalid method #2
			ExpectDiagnostic().WithLocation(10, 27),

			// Invalid method #3
			ExpectDiagnostic().WithLocation(15, 27),
			ExpectDiagnostic().WithLocation(16, 25),

			// Invalid method #4
			ExpectDiagnostic().WithLocation(19, 27),

			// Invalid method #5
			ExpectDiagnostic().WithLocation(25, 25),

			// Invalid method #6
			ExpectDiagnostic().WithLocation(29, 5),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Namespaces
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid namespace defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestNamespaceValidAsync()
	{
		var testCode = @"namespace ValidNamespace1
{
}

namespace ValidNamespace2
{
    using System;
}

namespace ValidNamespace3 { }

namespace ValidNamespace4 { using System; }

namespace ValidNamespace5
{ using System; }  
";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid namespace definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestNamespaceInvalidAsync()
	{
		var testCode = @"namespace InvalidNamespace1 {
}

namespace InvalidNamespace2 {
    using System;
}

namespace InvalidNamespace3 {
    using System; }

namespace InvalidNamespace4 { using System;
}

namespace InvalidNamespace5
{
    using System; }

namespace InvalidNamespace6
{ using System;
}
";

		var fixedTestCode = @"namespace InvalidNamespace1
{
}

namespace InvalidNamespace2
{
    using System;
}

namespace InvalidNamespace3
{
    using System;
}

namespace InvalidNamespace4
{
    using System;
}

namespace InvalidNamespace5
{
    using System;
}

namespace InvalidNamespace6
{
    using System;
}
";

		DiagnosticResult[] expectedDiagnostics =
		{
			// InvalidNamespace1
			ExpectDiagnostic().WithLocation(1, 29),

			// InvalidNamespace2
			ExpectDiagnostic().WithLocation(4, 29),

			// InvalidNamespace3
			ExpectDiagnostic().WithLocation(8, 29),
			ExpectDiagnostic().WithLocation(9, 19),

			// InvalidNamespace4
			ExpectDiagnostic().WithLocation(11, 29),

			// InvalidNamespace5
			ExpectDiagnostic().WithLocation(16, 19),

			// InvalidNamespace6
			ExpectDiagnostic().WithLocation(19, 1),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that an invalid namespace at the end of the source file will be handled correctly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestNamespaceInvalidAtEndOfFileAsync()
	{
		var testCode = @"
namespace TestNamespace
{
  using System; }";

		var fixedTestCode = @"
namespace TestNamespace
{
  using System;
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			ExpectDiagnostic().WithLocation(4, 17),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region ObjectInitializers
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid object initializers defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestObjectInitializersValidAsync()
	{
		var testCode = @"using System.Collections.Generic;
using System.Linq;

public class Foo
{
    // Valid object initializer #1
    private Contact[] contacts1 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers = 
            { 
                ""000-000-0000"", 
                ""000-000-0000"" 
            }
        }
    };

    // Valid object initializer #2
    private Contact[] contacts2 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" }
        }
    };

    // Valid object initializer #3
    private Contact[] contacts3 = new[]
    {
        new Contact { Name = ""Baz Foo"", PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" } }
    };

    // Valid object initializer #4
    private Contact[] contacts4 = new[] { new Contact { Name = ""Baz Foo"", PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" } } };

    public void Bar()
    {
        // Valid object initializer #5
        var contacts5 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers = 
                { 
                    ""000-000-0000"", 
                    ""000-000-0000"" 
                }
            }
        };

        // Valid object initializer #6
        var contacts6 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" }
            }
        };

        // Valid object initializer #7
        var contacts7 = new[]
        {
            new Contact { Name = ""Baz Foo"", PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" } }
        };

        // Valid object initializer #8
        var contacts8 = new[] { new Contact { Name = ""Baz Foo"", PhoneNumbers = { ""000-000-0000"", ""000-000-0000"" } } };

        // Valid object initializer #9
        var contacts9 = new
        { 
            Name = ""Foo Bar"", 
            PhoneNumbers = new[]
            {
                ""000-000-0000""
            }
        };

        // Valid object initializer #10
        var contacts10 = new
        { 
            Name = ""Foo Bar"", 
            PhoneNumbers = new[] { ""000-000-0000"" }
        };

        // Valid object initializer #11
        var contacts11 = new { Name = ""Foo Bar"", PhoneNumbers = new[] { ""000-000-0000"" } };    
    }

    // Valid object initializer #12
    public void Fish()
    {
        var someList = new List<int>
                        {
                            1,
                            2,
                            3
                        };
    }

    private class Contact
    {
        public string Name { get; set; }

        public List<string> PhoneNumbers { get; set; }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid object initializer definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestObjectInitializersInvalidAsync()
	{
		var testCode = @"using System.Collections.Generic;

public class Foo
{
    // Invalid object initializer #1
    private Contact[] contact1 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers =
            {
                ""000-000-0000"",
                ""000-000-0000"" } } };

    // Invalid object initializer #2
    private Contact[] contacts2 = new[]
    {   new Contact
        {   Name = ""Baz Foo"",
            PhoneNumbers =
            {   ""000-000-0000"",
                ""000-000-0000""
            }
        }
    };

    // Invalid object initializer #3
    private Contact[] contacts3 = new[] {
        new Contact {
            Name = ""Baz Foo"",
            PhoneNumbers = {
                ""000-000-0000"",
                ""000-000-0000""
            }
        }
    };

    public void Bar()
    {
        // Invalid object initializer #4
        var contact4 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers =
                {
                    ""000-000-0000"",
                    ""000-000-0000"" } } };

        // Invalid object initializer #5
        var contacts5 = new[]
        {   new Contact
            {   Name = ""Baz Foo"",
                PhoneNumbers =
                {   ""000-000-0000"",
                    ""000-000-0000""
                }
            }
        };

        // Invalid object initializer #6
        var contacts6 = new[] {
            new Contact {
                Name = ""Baz Foo"",
                PhoneNumbers = {
                    ""000-000-0000"",
                    ""000-000-0000""
                }
            }
        };

        // Invalid object initializer #7
        var contact7 = new
        {
            Name = ""Baz Foo"",
            PhoneNumbers = new[]
            {
                ""000-000-0000"" } };

        // Invalid object initializer #8
        var contacts8 = new
        {   Name = ""Baz Foo"",
            PhoneNumbers = new[]
            {   ""000-000-0000""
            }
        };

        // Invalid object initializer #9
        var contacts9 = new {
            Name = ""Baz Foo"",
            PhoneNumbers = new[] {
                ""000-000-0000""
            }
        };
    }

    private class Contact
    {
        public string Name { get; set; }

        public List<string> PhoneNumbers { get; set; }
    }
}";

		var fixedTestCode = @"using System.Collections.Generic;

public class Foo
{
    // Invalid object initializer #1
    private Contact[] contact1 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers =
            {
                ""000-000-0000"",
                ""000-000-0000""
            }
        }
    };

    // Invalid object initializer #2
    private Contact[] contacts2 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers =
            {
                ""000-000-0000"",
                ""000-000-0000""
            }
        }
    };

    // Invalid object initializer #3
    private Contact[] contacts3 = new[]
    {
        new Contact
        {
            Name = ""Baz Foo"",
            PhoneNumbers =
            {
                ""000-000-0000"",
                ""000-000-0000""
            }
        }
    };

    public void Bar()
    {
        // Invalid object initializer #4
        var contact4 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers =
                {
                    ""000-000-0000"",
                    ""000-000-0000""
                }
            }
        };

        // Invalid object initializer #5
        var contacts5 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers =
                {
                    ""000-000-0000"",
                    ""000-000-0000""
                }
            }
        };

        // Invalid object initializer #6
        var contacts6 = new[]
        {
            new Contact
            {
                Name = ""Baz Foo"",
                PhoneNumbers =
                {
                    ""000-000-0000"",
                    ""000-000-0000""
                }
            }
        };

        // Invalid object initializer #7
        var contact7 = new
        {
            Name = ""Baz Foo"",
            PhoneNumbers = new[]
            {
                ""000-000-0000""
            }
        };

        // Invalid object initializer #8
        var contacts8 = new
        {
            Name = ""Baz Foo"",
            PhoneNumbers = new[]
            {
                ""000-000-0000""
            }
        };

        // Invalid object initializer #9
        var contacts9 = new
        {
            Name = ""Baz Foo"",
            PhoneNumbers = new[]
            {
                ""000-000-0000""
            }
        };
    }

    private class Contact
    {
        public string Name { get; set; }

        public List<string> PhoneNumbers { get; set; }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid object initializer #1
			ExpectDiagnostic().WithLocation(14, 32),
			ExpectDiagnostic().WithLocation(14, 34),
			ExpectDiagnostic().WithLocation(14, 36),

			// Invalid object initializer #2
			ExpectDiagnostic().WithLocation(18, 5),
			ExpectDiagnostic().WithLocation(19, 9),
			ExpectDiagnostic().WithLocation(21, 13),

			// Invalid object initializer #3
			ExpectDiagnostic().WithLocation(28, 41),
			ExpectDiagnostic().WithLocation(29, 21),
			ExpectDiagnostic().WithLocation(31, 28),

			// Invalid object initializer #4
			ExpectDiagnostic().WithLocation(49, 36),
			ExpectDiagnostic().WithLocation(49, 38),
			ExpectDiagnostic().WithLocation(49, 40),

			// Invalid object initializer #5
			ExpectDiagnostic().WithLocation(53, 9),
			ExpectDiagnostic().WithLocation(54, 13),
			ExpectDiagnostic().WithLocation(56, 17),

			// Invalid object initializer #6
			ExpectDiagnostic().WithLocation(63, 31),
			ExpectDiagnostic().WithLocation(64, 25),
			ExpectDiagnostic().WithLocation(66, 32),

			// Invalid object initializer #7
			ExpectDiagnostic().WithLocation(79, 32),
			ExpectDiagnostic().WithLocation(79, 34),

			// Invalid object initializer #8
			ExpectDiagnostic().WithLocation(83, 9),
			ExpectDiagnostic().WithLocation(85, 13),

			// Invalid object initializer #9
			ExpectDiagnostic().WithLocation(90, 29),
			ExpectDiagnostic().WithLocation(92, 34),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that complex element initializers are handled properly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestComplexElementInitializerAsync()
	{
		var testCode = @"using System.Collections.Generic;

public class TestClass
{
    // Invalid object initializer #1
    private Dictionary<int, int> test1 = new Dictionary<int, int> {
        { 1, 1 }
    };

    // Invalid object initializer #2
    private Dictionary<int, int> test2 = new Dictionary<int, int>
    {
        { 1, 1 } };

    // Invalid object initializer #3
    private Dictionary<int, int> test3 = new Dictionary<int, int> {
        { 1, 1 } };
}
";

		var fixedCode = @"using System.Collections.Generic;

public class TestClass
{
    // Invalid object initializer #1
    private Dictionary<int, int> test1 = new Dictionary<int, int>
    {
        { 1, 1 }
    };

    // Invalid object initializer #2
    private Dictionary<int, int> test2 = new Dictionary<int, int>
    {
        { 1, 1 }
    };

    // Invalid object initializer #3
    private Dictionary<int, int> test3 = new Dictionary<int, int>
    {
        { 1, 1 }
    };
}
";

		DiagnosticResult[] expected =
		{
			// Invalid object initializer #1
			ExpectDiagnostic().WithLocation(6, 67),

			// Invalid object initializer #2
			ExpectDiagnostic().WithLocation(13, 18),

			// Invalid object initializer #3
			ExpectDiagnostic().WithLocation(16, 67),
			ExpectDiagnostic().WithLocation(17, 18),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	#endregion

	#region Properties
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid properties defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestPropertyValidAsync()
	{
		var testCode = @"using System;
using System.Collections.Generic;

public class Foo
{
    private bool test;

    // Valid property #1
    public bool Property1
    {
        get { return this.test; }
        set { this.test = value; }
    }

    // Valid property #2
    public bool Property2
    {
        get 
        { 
            return this.test; 
        }

        set 
        { 
            this.test = value; 
        }
    }

    // Valid property #3
    public bool Property3
    {
        get { return this.test; }
        
        set 
        { 
        }
    }

    // Valid property #4
    public bool Property4
    {
        get 
        { 
            return this.test; 
        }

        set { }
    }

    // Valid property #5
    public bool Property5 { get { return this.test; } }

    // Valid property #6
    public bool Property6 
    { get { return this.test; } }

    // Valid property #7
    public int[] Property7 { get; set; } = 
    { 
        0, 
        1, 
        2 
    };

    // Valid property #8
    public int[] Property8 { get; set; } = { 0, 1, 2 };
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid property definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestPropertyInvalidAsync()
	{
		var testCode = @"using System;

public class Foo
{
    private bool test;

    // Invalid property #1
    public bool Property1
    {
        get {
            return this.test;
        }

        set {
            this.test = value;
        }
    }

    // Invalid property #2
    public bool Property2
    {
        get {
            return this.test; }

        set {
            this.test = value; }
    }

    // Invalid property #3
    public bool Property3
    {
        get { return this.test;
        }

        set { this.test = value;
        }
    }

    // Invalid property #4
    public bool Property4
    {
        get
        {
            return this.test; }

        set
        {
            this.test = value; }
    }

    // Invalid property #5
    public bool Property5
    {
        get
        { return this.test;
        }

        set
        { this.test = value;
        }
    }

    // Invalid property #6
    public bool Property6
    {
        get
        { return this.test; }

        set
        { this.test = value; }
    }

    // Invalid property #7
    public bool Property7
    {
        get { return this.test; } }

    // Invalid property #8
    public bool Property8 {
        get { return this.test; } 
    }

    // Invalid property #9
    public bool Property9 {
        get { return this.test; } }

    // Invalid property #10
    public bool Property10 { get { return this.test; }
    }

    // Invalid property #11
    public bool Property11
    { get { return this.test; }
    }

    // Invalid property #12
    public int[] Property12 { get; set; } =
    {
        0,
        1,
        2 };

    // Invalid property #13
    public int[] Property13 { get; set; } = {
        0,
        1,
        2
    };

    // Invalid property #14
    public int[] Property14 { get; set; } = { 0, 1, 2
    };

    // Invalid property #15
    public int[] Property15 { get; set; } = 
    { 0, 1, 2 };
}";

		var fixedTestCode = @"using System;

public class Foo
{
    private bool test;

    // Invalid property #1
    public bool Property1
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid property #2
    public bool Property2
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid property #3
    public bool Property3
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid property #4
    public bool Property4
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid property #5
    public bool Property5
    {
        get
        {
            return this.test;
        }

        set
        {
            this.test = value;
        }
    }

    // Invalid property #6
    public bool Property6
    {
        get { return this.test; }

        set { this.test = value; }
    }

    // Invalid property #7
    public bool Property7
    {
        get { return this.test; }
    }

    // Invalid property #8
    public bool Property8
    {
        get { return this.test; } 
    }

    // Invalid property #9
    public bool Property9
    {
        get { return this.test; }
    }

    // Invalid property #10
    public bool Property10
    {
        get { return this.test; }
    }

    // Invalid property #11
    public bool Property11
    {
        get { return this.test; }
    }

    // Invalid property #12
    public int[] Property12 { get; set; } =
    {
        0,
        1,
        2
    };

    // Invalid property #13
    public int[] Property13 { get; set; } =
    {
        0,
        1,
        2
    };

    // Invalid property #14
    public int[] Property14 { get; set; } =
    {
        0, 1, 2
    };

    // Invalid property #15
    public int[] Property15 { get; set; } = 
    {
        0, 1, 2
    };
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid property #1
			ExpectDiagnostic().WithLocation(10, 13),
			ExpectDiagnostic().WithLocation(14, 13),

			// Invalid property #2
			ExpectDiagnostic().WithLocation(22, 13),
			ExpectDiagnostic().WithLocation(23, 31),
			ExpectDiagnostic().WithLocation(25, 13),
			ExpectDiagnostic().WithLocation(26, 32),

			// Invalid property #3
			ExpectDiagnostic().WithLocation(32, 13),
			ExpectDiagnostic().WithLocation(35, 13),

			// Invalid property #4
			ExpectDiagnostic().WithLocation(44, 31),
			ExpectDiagnostic().WithLocation(48, 32),

			// Invalid property #5
			ExpectDiagnostic().WithLocation(55, 9),
			ExpectDiagnostic().WithLocation(59, 9),

			// Invalid property #6 (Only report once for accessor statements on a single line)
			ExpectDiagnostic().WithLocation(67, 9),
			ExpectDiagnostic().WithLocation(70, 9),

			// Invalid property #7
			ExpectDiagnostic().WithLocation(76, 35),

			// Invalid property #8
			ExpectDiagnostic().WithLocation(79, 27),

			// Invalid property #9
			ExpectDiagnostic().WithLocation(84, 27),
			ExpectDiagnostic().WithLocation(85, 35),

			// Invalid property #10
			ExpectDiagnostic().WithLocation(88, 28),

			// Invalid property #11
			ExpectDiagnostic().WithLocation(93, 5),

			// Invalid property #12
			ExpectDiagnostic().WithLocation(101, 11),

			// Invalid property #13
			ExpectDiagnostic().WithLocation(104, 45),

			// Invalid property #14
			ExpectDiagnostic().WithLocation(111, 45),

			// Invalid property #15
			ExpectDiagnostic().WithLocation(116, 5),
			ExpectDiagnostic().WithLocation(116, 15),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	/// <summary>
	/// Verifies that a single line accessor with an embedded block will be handled correctly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestSingleLineAccessorWithEmbeddedBlockAsync()
	{
		var testCode = @"
public class TestClass
{
    public int[] TestProperty
    {
        get {
            {
                return new[] { 1, 2, 3 }; } }
    }
}
";

		var fixedTestCode = @"
public class TestClass
{
    public int[] TestProperty
    {
        get
        {
            {
                return new[] { 1, 2, 3 };
            }
        }
    }
}
";

		DiagnosticResult[] expected =
		{
				ExpectDiagnostic().WithLocation(6, 13),
				ExpectDiagnostic().WithLocation(8, 43),
				ExpectDiagnostic().WithLocation(8, 45),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	/// <summary>
	/// Verifies that a property declaration missing the opening brace will be handled correctly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestAccessorMissingOpeningBraceAsync()
	{
		var testCode = @"
class ClassName
{
    int Property
    {
        get
        }
    }
}";

		DiagnosticResult accessorError;
		{
			accessorError = DiagnosticResult.CompilerError("CS8180").WithMessage("{ or ; or => expected");
		}

		DiagnosticResult[] expected =
		{
			accessorError.WithLocation(6, 12),
			DiagnosticResult.CompilerError("CS1022").WithMessage("Type or namespace definition, or end-of-file expected").WithLocation(9, 1),
		};
		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	/// <summary>
	/// Verifies that a property declaration missing the closing brace at the end of the source file will be handled
	/// correctly.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestAccessorMissingClosingBraceAtEndOfFileAsync()
	{
		var testCode = @"
class ClassName
{
    int Property
    {
        get
        {";

		DiagnosticResult[] expected =
		{
			DiagnosticResult.CompilerError("CS0161").WithMessage("'ClassName.Property.get': not all code paths return a value").WithLocation(6, 9),
			DiagnosticResult.CompilerError("CS1513").WithMessage("} expected").WithLocation(7, 10),
			DiagnosticResult.CompilerError("CS1513").WithMessage("} expected").WithLocation(7, 10),
			DiagnosticResult.CompilerError("CS1513").WithMessage("} expected").WithLocation(7, 10),
		};
		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	#endregion

	#region StatementBlocks
	public static IEnumerable<object[]> StatementBlocksTokenList
	{
		get
		{
			yield return new[] { "checked" };
			yield return new[] { "fixed (int* p = new[] { 1, 2, 3 })" };
			yield return new[] { "for (var y = 0; y < 2; y++)" };
			yield return new[] { "foreach (var y in new[] { 1, 2, 3 })" };
			yield return new[] { "lock (this)" };
			yield return new[] { "unchecked" };
			yield return new[] { "unsafe" };
			yield return new[] { "using (var x = new System.Threading.ManualResetEvent(true))" };
			yield return new[] { "while (this.X < 2)" };
		}
	}

	/// <summary>
	/// Verifies that no diagnostics are reported for the valid statements defined in this test.
	/// </summary>
	/// <remarks>
	/// <para>The class is marked unsafe to make testing the fixed statement possible.</para>
	/// </remarks>
	/// <param name="token">The source code preceding the opening <c>{</c> of a statement block.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Theory]
	[MemberData(nameof(StatementBlocksTokenList))]
	public async Task TestStatementBlockValidAsync(string token)
	{
		var testCode = @"public unsafe class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // valid #1
        #TOKEN#
        {
        }

        // valid #2
        #TOKEN#
        {
            this.X = 1;
        }

        // valid #3
        #TOKEN# { }

        // valid #4
        #TOKEN# { this.X = 1; }

        // valid #5
        #TOKEN# 
        { this.X = 1; }
    }
}";

		testCode = testCode.Replace("#TOKEN#", token);

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid statements.
	/// </summary>
	/// <remarks>
	/// <para>The class is marked unsafe to make testing the fixed statement possible.</para>
	/// </remarks>
	/// <param name="token">The source code preceding the opening <c>{</c> of a statement block.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Theory]
	[MemberData(nameof(StatementBlocksTokenList))]
	public async Task TestStatementBlockInvalidAsync(string token)
	{
		var testCode = @"public unsafe class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // invalid #1
        #TOKEN# {
            this.X = 1;
        }

        // invalid #2
        #TOKEN# {
            this.X = 1; }

        // invalid #3
        #TOKEN# { this.X = 1;
        }

        // invalid #4
        #TOKEN#
        {
            this.X = 1; }

        // invalid #5
        #TOKEN#
        { this.X = 1;
        }
    }
}";

		var fixedTestCode = @"public unsafe class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // invalid #1
        #TOKEN#
        {
            this.X = 1;
        }

        // invalid #2
        #TOKEN#
        {
            this.X = 1;
        }

        // invalid #3
        #TOKEN#
        {
            this.X = 1;
        }

        // invalid #4
        #TOKEN#
        {
            this.X = 1;
        }

        // invalid #5
        #TOKEN#
        {
            this.X = 1;
        }
    }
}";

		testCode = testCode.Replace("#TOKEN#", token);
		fixedTestCode = fixedTestCode.Replace("#TOKEN#", token);
		var tokenLength = token.Length;

		DiagnosticResult[] expectedDiagnostics =
		{
			// invalid #1
			ExpectDiagnostic().WithLocation(8, 10 + tokenLength),

			// invalid #2
			ExpectDiagnostic().WithLocation(13, 10 + tokenLength),
			ExpectDiagnostic().WithLocation(14, 25),

			// invalid #3
			ExpectDiagnostic().WithLocation(17, 10 + tokenLength),

			// invalid #4
			ExpectDiagnostic().WithLocation(23, 25),

			// invalid #5
			ExpectDiagnostic().WithLocation(27, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region Switches
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid switch statements defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestSwitchValidAsync()
	{
		var testCode = @"public class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // valid switch #1
        switch (this.X)
        {
            case 0:
                break;
        }

        // valid switch #2
        switch (this.X) { case 0: break; }

        // valid switch #3
        switch (this.X) 
        { case 0: break; }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid switch statements.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestSwitchInvalidAsync()
	{
		var testCode = @"public class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // invalid switch #1
        switch (this.X) {
            case 0:
                break;
        }

        // invalid switch #2
        switch (this.X) {
            case 0:
                break; }

        // invalid switch #3
        switch (this.X) { case 0:
                break;
        }

        // invalid switch #4
        switch (this.X)
        {
            case 0:
                break; }

        // invalid switch #5
        switch (this.X)
        { case 0:
                break;
        }
    }
}";

		var fixedTestCode = @"public class Foo
{
    public int X { get; set; }

    public void Bar()
    {
        // invalid switch #1
        switch (this.X)
        {
            case 0:
                break;
        }

        // invalid switch #2
        switch (this.X)
        {
            case 0:
                break;
        }

        // invalid switch #3
        switch (this.X)
        {
            case 0:
                break;
        }

        // invalid switch #4
        switch (this.X)
        {
            case 0:
                break;
        }

        // invalid switch #5
        switch (this.X)
        {
            case 0:
                break;
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// invalid switch #1
			ExpectDiagnostic().WithLocation(8, 25),

			// invalid switch #2
			ExpectDiagnostic().WithLocation(14, 25),
			ExpectDiagnostic().WithLocation(16, 24),

			// invalid switch #3
			ExpectDiagnostic().WithLocation(19, 25),

			// invalid switch #4
			ExpectDiagnostic().WithLocation(27, 24),

			// invalid switch #5
			ExpectDiagnostic().WithLocation(31, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion

	#region TryCatchFinallys
	/// <summary>
	/// Verifies that no diagnostics are reported for the valid try ... catch ... finally statements defined in this test.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestTryCatchFinallyValidAsync()
	{
		var testCode = @"using System;

public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Valid try ... catch ... finally #1
        try
        {
        }
        catch (Exception)
        {
        }
        finally
        {
        }

        // Valid try ... catch ... finally #2
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }

        // Valid try ... catch ... finally #3
        try { } catch (Exception) { }

        // Valid try ... catch ... finally #4
        try { x += 1; } catch (Exception) { x += 2; }

        // Valid try ... catch ... finally #5
        try { } finally { }

        // Valid try ... catch ... finally #6
        try { x += 1; } finally { x += 3; }

        // Valid try ... catch ... finally #7
        try { x += 1; } catch (Exception) { x += 2; } finally { x += 3; }

        // Valid try ... catch ... finally #8
        try { x += 1; } catch (Exception) { x += 2; } finally { x += 3; }

        // Valid try ... catch ... finally #9
        try 
        { x += 1; } 
        catch (Exception) 
        { x += 2; } 
        finally 
        { x += 3; }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies that diagnostics will be reported for all invalid try ... catch ... finally statement definitions.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task TestTryCatchFinallyInvalidAsync()
	{
		var testCode = @"using System;

public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid try ... catch ... finally #1
        try {
        }
        catch (Exception) {
        }
        finally {
        }

        // Invalid try ... catch ... finally #2
        try {
            x += 1;
        }
        catch (Exception) {
            x += 2;
        }
        finally {
            x += 3;
        }

        // Invalid try ... catch ... finally #3
        try {
            x += 1; }
        catch (Exception) {
            x += 2; }
        finally {
            x += 3; }

        // Invalid try ... catch ... finally #4
        try { x += 1;
        }
        catch (Exception) { x += 2;
        }
        finally { x += 3;
        }

        // Invalid try ... catch ... finally #5
        try
        {
            x += 1; }
        catch (Exception)
        {
            x += 2; }
        finally
        {
            x += 3; }

        // Invalid try ... catch ... finally #6
        try
        { x += 1;
        }
        catch (Exception)
        { x += 2;
        }
        finally
        { x += 3;
        }
    }
}";

		var fixedTestCode = @"using System;

public class Foo
{
    private void Bar()
    {
        var x = 0;

        // Invalid try ... catch ... finally #1
        try
        {
        }
        catch (Exception)
        {
        }
        finally
        {
        }

        // Invalid try ... catch ... finally #2
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }

        // Invalid try ... catch ... finally #3
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }

        // Invalid try ... catch ... finally #4
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }

        // Invalid try ... catch ... finally #5
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }

        // Invalid try ... catch ... finally #6
        try
        {
            x += 1;
        }
        catch (Exception)
        {
            x += 2;
        }
        finally
        {
            x += 3;
        }
    }
}";

		DiagnosticResult[] expectedDiagnostics =
		{
			// Invalid try ... catch ... finally #1
			ExpectDiagnostic().WithLocation(10, 13),
			ExpectDiagnostic().WithLocation(12, 27),
			ExpectDiagnostic().WithLocation(14, 17),

			// Invalid try ... catch ... finally #2
			ExpectDiagnostic().WithLocation(18, 13),
			ExpectDiagnostic().WithLocation(21, 27),
			ExpectDiagnostic().WithLocation(24, 17),

			// Invalid try ... catch ... finally #3
			ExpectDiagnostic().WithLocation(29, 13),
			ExpectDiagnostic().WithLocation(30, 21),
			ExpectDiagnostic().WithLocation(31, 27),
			ExpectDiagnostic().WithLocation(32, 21),
			ExpectDiagnostic().WithLocation(33, 17),
			ExpectDiagnostic().WithLocation(34, 21),

			// Invalid try ... catch ... finally #4
			ExpectDiagnostic().WithLocation(37, 13),
			ExpectDiagnostic().WithLocation(39, 27),
			ExpectDiagnostic().WithLocation(41, 17),

			// Invalid try ... catch ... finally #5
			ExpectDiagnostic().WithLocation(47, 21),
			ExpectDiagnostic().WithLocation(50, 21),
			ExpectDiagnostic().WithLocation(53, 21),

			// Invalid try ... catch ... finally #6
			ExpectDiagnostic().WithLocation(57, 9),
			ExpectDiagnostic().WithLocation(60, 9),
			ExpectDiagnostic().WithLocation(63, 9),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expectedDiagnostics, fixedTestCode);
	}

	#endregion
}
