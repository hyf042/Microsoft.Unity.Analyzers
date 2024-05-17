#nullable disable

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0005ElementMustNamedUpperPascalCaseAnalyzer"/>.
/// </summary>
public class BEY0005ElementMustNamedUpperPascalCaseTests : BaseCodeFixVerifierTest<BEY0005ElementMustNamedUpperPascalCaseAnalyzer, BEY0005ElementMustNamedUpperPascalCaseCodeFix>
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

	[Fact]
	public async Task TestUpperCaseNamespaceAsync()
	{
		var testCode = @"namespace Test
{ 

}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseNamespaceAsync()
	{
		var testCode = @"namespace test
{ 

}";

		var fixedCode = @"namespace Test
{ 

}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 11);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseComplicatedNamespaceAsync()
	{
		var testCode = @"namespace test.foo.bar
{

}";

		var fixedCode = @"namespace Test.Foo.Bar
{

}";

		DiagnosticResult[] expected = new[]
		{
			ExpectDiagnostic().WithArguments("test").WithLocation(1, 11),
			ExpectDiagnostic().WithArguments("foo").WithLocation(1, 16),
			ExpectDiagnostic().WithArguments("bar").WithLocation(1, 20),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseClassAsync()
	{
		var testCode = @"public class Test
{ 

}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseClassAsync()
	{
		var testCode = @"public class test
{ 

}";
		var fixedCode = @"public class Test
{ 

}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 14);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseClassWithConflictAsync()
	{
		var testCode = @"public class test
{
}

public class Test { }";
		var fixedCode = @"public class Test1
{
}

public class Test { }";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 14);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseInterfaceAsync()
	{
		var testCode = @"public interface Test
{

}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseInterfaceAsync()
	{
		var testCode = @"public interface test
{

}";

		// Reported as BEY0007
		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseStructAsync()
	{
		var testCode = @"public struct Test 
{ 

}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseStructAsync()
	{
		var testCode = @"public struct test 
{ 

}";
		var fixedCode = @"public struct Test 
{ 

}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 15);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseStructWithConflictAsync()
	{
		var testCode = @"public struct test
{
}

public class Test { }";
		var fixedCode = @"public struct Test1
{
}

public class Test { }";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 15);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseEnumAsync()
	{
		var testCode = @"public enum Test 
{ 

}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumAsync()
	{
		var testCode = @"public enum test 
{ 

}";
		var fixedCode = @"public enum Test 
{ 

}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumWithConflictAsync()
	{
		var testCode = @"public enum test
{
}

public class Test { }";
		var fixedCode = @"public enum Test1
{
}

public class Test { }";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumWithMemberMatchingTargetNameAsync()
	{
		var testCode = @"public enum test
{
    Test
}";
		var fixedCode = @"public enum Test
{
    Test
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(1, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseEnumMemberAsync()
	{
		var testCode = @"public enum Test
{
    Member
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumMemberAsync()
	{
		var testCode = @"public enum Test
{
    member
}";
		var fixedCode = @"public enum Test
{
    Member
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("member").WithLocation(3, 5);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumMemberWithConflictAsync()
	{
		var testCode = @"public enum Test
{
    member,
    Member
}";
		var fixedCode = @"public enum Test
{
    Member1,
    Member
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("member").WithLocation(3, 5);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumMemberWithTwoConflictsAsync()
	{
		var testCode = @"public enum Test
{
    member,
    Member,
    Member1,
}";
		var fixedCode = @"public enum Test
{
    Member2,
    Member,
    Member1,
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("member").WithLocation(3, 5);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEnumMemberWithNumberAndConflictAsync()
	{
		var testCode = @"public enum Test
{
    member1,
    Member1
}";
		var fixedCode = @"public enum Test
{
    Member11,
    Member1
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("member1").WithLocation(3, 5);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseDelegateAsync()
	{
		var testCode = @"public class TestClass
{ 
public delegate void Test();
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseDelegateAsync()
	{
		var testCode = @"public class TestClass
{ 
public delegate void test();
}";
		var fixedCode = @"public class TestClass
{ 
public delegate void Test();
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(3, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseDelegateWithConflictAsync()
	{
		var testCode = @"public class Test1
{
public delegate void test();

public int Test => 0;
}";
		var fixedCode = @"public class Test1
{
public delegate void Test2();

public int Test => 0;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(3, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseEventAsync()
	{
		var testCode = @"public class TestClass
{
    public delegate void Test();
    Test _testEvent;
    public event Test TestEvent
    {
        add
        {
            _testEvent += value;
        }
        remove
        {
            _testEvent -= value;
        }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseEventFieldAsync()
	{
		var testCode = @"public class TestClass
{
    public delegate void Test();
    public event Test TestEvent;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseMethodAsync()
	{
		var testCode = @"public class TestClass
{
public void Test()
{
}
private void test() // private method is handled by BEY0008
{
}
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseMethodAsync()
	{
		var testCode = @"public class TestClass
{
public void test()
{
}
}";
		var fixedCode = @"public class TestClass
{
public void Test()
{
}
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(3, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseMethodWithConflictAsync()
	{
		// Conflict resolution does not attempt to examine overloaded methods.
		var testCode = @"public class TestClass
{
public void test()
{
}

public int Test(int value) => value;
}";
		var fixedCode = @"public class TestClass
{
public void Test1()
{
}

public int Test(int value) => value;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("test").WithLocation(3, 13);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCasePropertyAsync()
	{
		var testCode = @"public class TestClass
{
public string Test { get; set; }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCasePublicFieldAsync()
	{
		var testCode = @"public class TestClass
{
public string Test;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseInternalFieldAsync()
	{
		var testCode = @"public class TestClass
{
internal string Test;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseConstFieldAsync()
	{
		var testCode = @"public class TestClass
{
const string Test = ""value"";
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestUpperCaseProtectedReadOnlyFieldAsync()
	{
		var testCode = @"public class TestClass
{
protected readonly string Test;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseProtectedFieldAsync()
	{
		var testCode = @"public class TestClass
{
protected string Test;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseReadOnlyFieldAsync()
	{
		var testCode = @"public class TestClass
{
readonly string test;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCasePublicFieldAsync()
	{
		var testCode = @"public class TestClass
{
public string test;
}";

		// Handled by BEY0004
		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseInternalFieldAsync()
	{
		var testCode = @"public class TestClass
{
internal string test;
}";

		// Handled by BEY0004
		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseConstFieldAsync()
	{
		var testCode = @"public class TestClass
{
const string test = ""value"";
}";

		// Reported as BEY0006
		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestNativeMethodsExceptionAsync()
	{
		var testCode = @"public class TestNativeMethods
{
public string test { get; set; }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseProtectedReadOnlyFieldAsync()
	{
		var testCode = @"public class TestClass
{
protected readonly string test;
}";

		// Handled by BEY0010
		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLowerCaseOverriddenMembersAsync()
	{
		var testCode = @"public class TestClass : BaseClass
{
    public override int bar
    {
        get
        {
            return 0;
        }
    }

    public override event System.EventHandler fooBar
    {
        add { }
        remove { }
    }

    public override void foo()
    {

    }
}

public abstract class BaseClass
{
    public abstract void foo();
    public abstract int bar { get; }
    public abstract event System.EventHandler fooBar;
}";
		var fixedCode = @"public class TestClass : BaseClass
{
    public override int bar
    {
        get
        {
            return 0;
        }
    }

    public override event System.EventHandler fooBar
    {
        add { }
        remove { }
    }

    public override void Foo()
    {

    }
}

public abstract class BaseClass
{
    public abstract void Foo();
    public abstract int bar { get; }
    public abstract event System.EventHandler fooBar;
}";

		var expected = ExpectDiagnostic().WithLocation(25, 26).WithArguments("foo");

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseInterfaceMembersAsync()
	{
		var testCode = @"public class TestClass : IInterface
{
    public void foo()
    {
    }
}

public interface IInterface
{
    void foo();
}";
		var fixedCode = @"public class TestClass : IInterface
{
    public void Foo()
    {
    }
}

public interface IInterface
{
    void Foo();
}";

		var expected = ExpectDiagnostic().WithLocation(10, 10).WithArguments("foo");

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUnderscoreExclusionAsync()
	{
		var testCode = @"public enum TestEnum
{
    _12clock,
    _12Clock,
    _tick,
    _Tock,
}
";

		var fixedCode = @"public enum TestEnum
{
    _12clock,
    _12Clock,
    Tick,
    Tock,
}
";

		DiagnosticResult[] expected =
		{
				ExpectDiagnostic().WithLocation(5, 5).WithArguments("_tick"),
				ExpectDiagnostic().WithLocation(6, 5).WithArguments("_Tock"),
			};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Theory]
	[InlineData("_")]
	[InlineData("__")]
	public async Task TestUnderscoreMethodAsync(string name)
	{
		var testCode = $@"
public class TestClass
{{
    public void {name}()
    {{
    }}
}}";

		var fixedCode = $@"
public class TestClass
{{
    public void {name}()
    {{
    }}
}}";

		await VerifyCSharpFixAsync(testCode, fixedCode);
	}
}
