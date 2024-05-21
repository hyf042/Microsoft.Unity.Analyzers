using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0004ElementMustNamedLowerCamelCaseAnalyzer"/>.
/// </summary>
public class BEY0004ElementMustNamedLowerCamelCaseTests : BaseCodeFixVerifierTest<BEY0004ElementMustNamedLowerCamelCaseAnalyzer, BEY0004ElementMustNamedLowerCamelCaseCodeFix>
{
	protected override bool ExpectErrorAsDiagnosticResult => true;

	protected override string[] DisabledDiagnostics
	{
		get =>
		[ 
			// Suppress CS0067: warning CS0067: The event 'TypeName.bar' is never used
			"CS0067",
			// Suppress CS0108: warning CS0108: 'IDerivedTest.Method(int, int, int)' hides inherited member 'ITest.Method(int, int, int)'. Use the new keyword if hiding was intended.
			"CS0108",
			// Suppress CS0168: warning CS0168: The variable 'Variable' is declared but never used
			"CS0168",
			// Suppress CS0169: warning CS0169: The field 'Foo.Bar' is never used
			"CS0169",
			// Suppress CS0219: The variable 'Constant' is assigned but its value is never used
			"CS0219",
			// Suppress CS0649: warning CS0649: Field 'Foo.Bar' is never assigned to, and will always have its default value null
			"CS0649",
			// Suppress CS8019: hidden CS8019: Unnecessary using directive.
			"CS8019",
		];
	}

	[Theory]
	[InlineData("")]
	[InlineData("const")]
	[InlineData("private const")]
	[InlineData("internal const")]
	[InlineData("protected const")]
	[InlineData("protected internal const")]
	[InlineData("public const")]
	[InlineData("protected readonly")]
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

	[Theory]
	[InlineData("public")]
	[InlineData("public readonly")]
	[InlineData("internal")]
	[InlineData("internal readonly")]
	public async Task TestThatDiagnosticIsReported_AllowedVariablePrefixesAsync(string modifiers)
	{
		var testCode = @"public class Foo
{{
{0}
string UI = """", _UI = """", __UI = """";
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReported_EventAsync()
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
    public event Test testEvent
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
	public async Task TestThatDiagnosticIsNotReported_PropertyAsync()
	{
		var testCode = @"public class TestClass
{
public string Test { get; set; }
public string test { get; set; }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReported_EventFieldAsync()
	{
		var testCode = @"public class TestClass
{
    public delegate void Test();
    public event Test TestEvent;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReported_DelegateTypeFieldAsync()
	{
		var testCode = @"public class TestClass
{
    public delegate void Test();
    public Test TestEvent;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Theory]
	[InlineData("public")]
	[InlineData("public readonly")]
	[InlineData("internal")]
	[InlineData("internal readonly")]
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
			ExpectDiagnostic().WithArguments("Bar").WithLocation(4, 8),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(8, 8),
			ExpectDiagnostic().WithArguments("_Far").WithLocation(12, 8),
			ExpectDiagnostic().WithArguments("__Har").WithLocation(16, 8),
			ExpectDiagnostic().WithArguments("___Jar").WithLocation(20, 8),
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

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("public")]
	[InlineData("internal")]
	[InlineData("public readonly")]
	[InlineData("internal readonly")]
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
			ExpectDiagnostic().WithArguments("Dar").WithLocation(4, 18),
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

		var expected = ExpectDiagnostic().WithArguments("Test").WithLocation(4, 19);
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

	[Fact]
	public async Task TestFieldPlacedInsideAttributeDerivedClassAsync()
	{
		var testCode = @"public class FooAttribute : System.Attribute
{
    public string Bar = ""baz"";
    public int Car => 1;

    public delegate void Test();
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
    Test _testEvent;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	// Uncomment the below four tests when we want property and event diagnostic back
	/*[Fact]
	public async Task TestLowerCaseEventFieldWithConflictAsync()
	{
		var testCode = @"public class TestClass
{
    public delegate void Test();
    public event Test testEvent;
    public event Test TestEvent;
}";
		var fixedCode = @"public class TestClass
{
    public delegate void Test();
    public event Test testEvent;
    public event Test testEvent1;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("TestEvent").WithLocation(5, 23);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCaseEventWithConflictAsync()
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

    public int testEvent => 0;
}";
		var fixedCode = @"public class TestClass
{
    public delegate void Test();
    Test _testEvent;
    public event Test testEvent1
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

    public int testEvent => 0;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("testEvent").WithLocation(5, 23);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestLowerCasePropertyWithConflictAsync()
	{
		var testCode = @"public class TestClass
{
public string Test { get; set; }
public string test => string.Empty;
}";
		var fixedCode = @"public class TestClass
{
public string test1 { get; set; }
public string test => string.Empty;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Test").WithLocation(3, 15);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestUpperCaseInterfaceMembersAsync()
	{
		var testCode = @"public class TestClass : IInterface
{
    public int Bar
    {
        get
        {
            return 0;
        }
    }

    public event System.EventHandler FooBar
    {
        add { }
        remove { }
    }

    public string IInterface { get; }
}

public interface IInterface
{
    int Bar { get; }
    event System.EventHandler FooBar;
    string IInterface { get; }
}";
		var fixedCode = @"public class TestClass : IInterface
{
    public int bar
    {
        get
        {
            return 0;
        }
    }

    public event System.EventHandler fooBar
    {
        add { }
        remove { }
    }

    public string iInterface { get; }
}

public interface IInterface
{
    int bar { get; }
    event System.EventHandler fooBar;
    string iInterface { get; }
}";

		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(22, 9).WithArguments("bar"),
			ExpectDiagnostic().WithLocation(23, 31).WithArguments("fooBar"),
			ExpectDiagnostic().WithLocation(24, 12).WithArguments("iInterface"),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}*/

	[Fact]
	public async Task TestThatDiagnosticIsNotReportedForConstVariableAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        const string Bar = """", car = """", Dar = """";
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsNotReportedForUnderscoreOnlyNames()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(int parameter)
    {
        string _ = parameter.ToString();
        string __ = parameter.ToString();
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsReported_SingleVariableAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string Bar;
        string car;
        string Par;
    }
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(5, 16),
			ExpectDiagnostic().WithArguments("Par").WithLocation(7, 16),
		};

		var fixedCode = @"public class TypeName
{
    public void MethodName()
    {
        string bar;
        string car;
        string par;
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsReported_MultipleVariablesAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string Bar, car, Par;
    }
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(5, 16),
			ExpectDiagnostic().WithArguments("Par").WithLocation(5, 26),
		};

		var fixedCode = @"public class TypeName
{
    public void MethodName()
    {
        string bar, car, par;
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestVariableStartingWithAnUnderscoreAndUpperLetterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string _bar = ""baz"";
        string _Car = ""caz"";
    }
}";

		var fixedTestCode = @"public class TypeName
{
    public void MethodName()
    {
        string _bar = ""baz"";
        string _car = ""caz"";
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("_Car").WithLocation(6, 16);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedTestCode);
	}

	[Fact]
	public async Task TestVariableStartingWithLetterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string bar = ""baz"";
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestVariableInCatchDeclarationAsync()
	{
		var testCode = @"
using System;
public class TypeName
{
    public void MethodName()
    {
        try
        {
        }
        catch (Exception Ex)
        {
        }
    }
}";
		var fixedCode = @"
using System;
public class TypeName
{
    public void MethodName()
    {
        try
        {
        }
        catch (Exception ex)
        {
        }
    }
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Ex").WithLocation(10, 26),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestVariableInForEachStatementAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        foreach (var X in new int[0])
        {
        }
    }
}";
		var fixedCode = @"public class TypeName
{
    public void MethodName()
    {
        foreach (var x in new int[0])
        {
        }
    }
}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("X").WithLocation(5, 22),
		};

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestVariablePlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    public void MethodName()
    {
        string Bar = ""baz"";
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithVariableAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string variable = ""Text"";
        string Variable = variable.ToString();
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Variable").WithLocation(6, 16);

		var fixedCode = @"public class TypeName
{
    public void MethodName()
    {
        string variable = ""Text"";
        string variable1 = variable.ToString();
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithVariableKeywordAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        string Int;
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Int").WithLocation(5, 16);

		var fixedCode = @"public class TypeName
{
    public void MethodName()
    {
        string @int;
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithParameterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(int parameter)
    {
        string Parameter = parameter.ToString();
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Parameter").WithLocation(5, 16);

		var fixedCode = @"public class TypeName
{
    public void MethodName(int parameter)
    {
        string parameter1 = parameter.ToString();
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsReported_SingleParameterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string Bar)
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Bar").WithLocation(3, 35);

		var fixedCode = @"public class TypeName
{
    public void MethodName(string bar)
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestThatDiagnosticIsReported_MultipleParametersAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string Bar, string car, string Par)
    {
    }
}";

		DiagnosticResult[] expected =
		{
				ExpectDiagnostic().WithArguments("Bar").WithLocation(3, 35),
				ExpectDiagnostic().WithArguments("Par").WithLocation(3, 59),
			};

		var fixedCode = @"public class TypeName
{
    public void MethodName(string bar, string car, string par)
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestParameterStartingWithLetterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string bar)
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestParameterPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"public class FooNativeMethods
{
    public void MethodName(string Bar)
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithKeywordAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string Int)
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Int").WithLocation(3, 35);

		var fixedCode = @"public class TypeName
{
    public void MethodName(string @int)
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithLaterParameterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string Parameter, int parameter)
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Parameter").WithLocation(3, 35);

		var fixedCode = @"public class TypeName
{
    public void MethodName(string parameter1, int parameter)
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestRenameConflictsWithEarlierParameterAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(string parameter, int Parameter)
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Parameter").WithLocation(3, 50);

		var fixedCode = @"public class TypeName
{
    public void MethodName(string parameter, int parameter1)
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestNoViolationOnInterfaceParameterNameAsync()
	{
		var testCode = @"
public interface ITest
{
    void Method(int Param1, int param2, int Param3);
}

public class Test : ITest
{
    public void Method(int Param1, int param2, int param3)
    {
    }
}";
		var expected = new[]
		{
				ExpectDiagnostic().WithLocation(4, 21).WithArguments("Param1"),
				ExpectDiagnostic().WithLocation(4, 45).WithArguments("Param3"),
			};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestViolationOnRenamedInterfaceParameterNameAsync()
	{
		var testCode = @"
public interface ITest
{
    void Method(int Param1, int param2, int Param3);
}

public class Test : ITest
{
    public void Method(int Param1, int param2, int Other)
    {
    }
}";

		var expected = new[]
		{
				ExpectDiagnostic().WithLocation(4, 21).WithArguments("Param1"),
				ExpectDiagnostic().WithLocation(4, 45).WithArguments("Param3"),
				ExpectDiagnostic().WithLocation(9, 52).WithArguments("Other"),
			};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestNoViolationOnExplicitlyImplementedInterfaceParameterNameAsync()
	{
		var testCode = @"
public interface ITest
{
    void Method(int param1, int Param2);
}

public class Test : ITest
{
    void ITest.Method(int param1, int Param2)
    {
    }
}";

		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(4, 33).WithArguments("Param2"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestViolationOnRenamedExplicitlyImplementedInterfaceParameterNameAsync()
	{
		var testCode = @"
public interface ITest
{
    void Method(int param1, int Param2);
}

public class Test : ITest
{
    public void Method(int param1, int Param2)
    {
    }
}";

		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(4, 33).WithArguments("Param2"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestNoViolationOnAbstractParameterNameAsync()
	{
		var testCode = @"
public abstract class TestBase
{
    public abstract void Method(int Param1, int param2, int Param3);
}

public class Test : TestBase
{
    public override void Method(int Param1, int param2, int param3)
    {
    }
}";

		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(4, 37).WithArguments("Param1"),
			ExpectDiagnostic().WithLocation(4, 61).WithArguments("Param3"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestViolationOnRenamedAbstractParameterNameAsync()
	{
		var testCode = @"
public abstract class Testbase
{
    public abstract void Method(int Param1, int param2, int Param3);
}

public class Test : Testbase
{
    public override void Method(int Param1, int param2, int Other)
    {
    }
}";

		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(4, 37).WithArguments("Param1"),
			ExpectDiagnostic().WithLocation(4, 61).WithArguments("Param3"),
			ExpectDiagnostic().WithLocation(9, 61).WithArguments("Other"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestSimpleLambaExpressionAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        System.Action<int> action = Ignored => { };
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithArguments("Ignored").WithLocation(5, 37);

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestLambdaParameterNamedUnderscoreAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        System.Action<int> action1 = _ => { };
        System.Action<int> action2 = (_) => { };
        System.Action<int> action3 = delegate(int _) { };
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies this diagnostic does not check whether or not a parameter named <c>_</c> is being used.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task TestLambdaParameterNamedUnderscoreUsageAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        System.Func<int, int> function1 = _ => _;
        System.Func<int, int> function2 = (_) => _;
        System.Func<int, int> function3 = delegate(int _) { return _; };
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestLambdaParameterNamedDoubleUnderscoreAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        System.Action<int> action1 = __ => { };
        System.Action<int> action2 = (__) => { };
        System.Action<int> action3 = delegate(int __) { };
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	/// <summary>
	/// Verifies this diagnostic does not check whether or not a parameter named <c>__</c> is being used.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[Fact]
	public async Task TestLambdaParameterNamedDoubleUnderscoreUsageAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName()
    {
        System.Func<int, int> function1 = __ => __;
        System.Func<int, int> function2 = (__) => __;
        System.Func<int, int> function3 = delegate(int __) { return __; };
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestMethodParameterNamedUnderscoreAsync()
	{
		var testCode = @"public class TypeName
{
    public void MethodName(int _, int __)
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestInheritedInterfacesWithOverloadedMembersAsync()
	{
		var testCode = @"
public interface ITest
{
    void Method(int Param1, int param2, int Param3);
    void Method();
}

public interface IEmptyInterface { }

public interface IDerivedTest : ITest, IEmptyInterface
{
    void Method(int Param1, int param2, int param3);
}";
		var expected = new[]
		{
			ExpectDiagnostic().WithLocation(4, 21).WithArguments("Param1"),
			ExpectDiagnostic().WithLocation(4, 45).WithArguments("Param3"),
			ExpectDiagnostic().WithLocation(12, 21).WithArguments("Param1"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	/// <summary>
	/// Verify that an invalid method override will not produce a diagnostic nor crash the analyzer.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
	[Fact]
	public async Task InvalidMethodOverrideShouldNotProduceDiagnosticAsync()
	{
		var testCode = @"
namespace TestNamespace
{
    public abstract class BaseClass
    {
        public abstract void TestMethod(int p1, int p2);
    }

    public class TestClass : BaseClass
    {
        public override void TestMethod(int p1, X int P2)
        {
        }
    }
}
";
		DiagnosticResult[] expected =
		{
			DiagnosticResult.CompilerError("CS0534").WithLocation(9, 18).WithMessage("'TestClass' does not implement inherited abstract member 'BaseClass.TestMethod(int, int)'"),
			DiagnosticResult.CompilerError("CS0246").WithLocation(11, 49).WithMessage("The type or namespace name 'X' could not be found (are you missing a using directive or an assembly reference?)"),
			DiagnosticResult.CompilerError("CS1001").WithLocation(11, 51).WithMessage("Identifier expected"),
			DiagnosticResult.CompilerError("CS1003").WithLocation(11, 51).WithMessage("Syntax error, ',' expected"),
		};

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}
}
