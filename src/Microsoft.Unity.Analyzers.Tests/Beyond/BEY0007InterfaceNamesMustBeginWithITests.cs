using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0007InterfaceNamesMustBeginWithIAnalyzer"/>.
/// </summary>
public class BEY0007InterfaceNamesMustBeginWithITests : BaseCodeFixVerifierTest<BEY0007InterfaceNamesMustBeginWithIAnalyzer, BEY0007InterfaceNamesMustBeginWithICodeFix>
{
	[Fact]
	public async Task TestInterfaceDeclarationDoesNotStartWithIAsync()
	{
		var testCode = @"
public interface Foo
{
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(2, 18);

		var fixedCode = @"
public interface IFoo
{
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestInterfaceDeclarationDoesNotStartWithIPlusInterfaceUsedAsync()
	{
		var testCode = @"
public interface Foo
{
}
public class Bar : Foo
{
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(2, 18);

		var fixedCode = @"
public interface IFoo
{
}
public class Bar : IFoo
{
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestInterfaceDeclarationStartsWithLowerIAsync()
	{
		var testCode = @"
public interface iFoo
{
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(2, 18);

		var fixedCode = @"
public interface IiFoo
{
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestInnerInterfaceDeclarationDoesNotStartWithIAsync()
	{
		var testCode = @"
public class Bar
{
    public interface Foo
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(4, 22);

		await VerifyCSharpDiagnosticAsync(testCode, expected);
	}

	[Fact]
	public async Task TestInterfaceDeclarationDoesStartWithIAsync()
	{
		var testCode = @"public interface IFoo
{
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestInnerInterfaceDeclarationDoesStartWithIAsync()
	{
		var testCode = @"
public class Bar
{
    public interface IFoo
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestComInterfaceInNativeMethodsClassAsync()
	{
		var testCode = @"
using System.Runtime.InteropServices;
public class NativeMethods
{
    [ComImport, Guid(""C8123315-D374-4DB8-9E7A-CB3499E46F2C"")]
    public interface FileOpenDialog
    {
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestComInterfaceInNativeMethodsClassWithIncorrectNameAsync()
	{
		var testCode = @"
using System.Runtime.InteropServices;
public class NativeMethodsClass
{
    [ComImport, Guid(""C8123315-D374-4DB8-9E7A-CB3499E46F2C"")]
    public interface FileOpenDialog
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(6, 22);

		var fixedCode = @"
using System.Runtime.InteropServices;
public class NativeMethodsClass
{
    [ComImport, Guid(""C8123315-D374-4DB8-9E7A-CB3499E46F2C"")]
    public interface IFileOpenDialog
    {
    }
}";

		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestComInterfaceInInnerClassInNativeMethodsClassAsync()
	{
		var testCode = @"
using System.Runtime.InteropServices;
public class MyNativeMethods
{
    public class FileOperations
    {
        [ComImport, Guid(""C8123315-D374-4DB8-9E7A-CB3499E46F2C"")]
        public interface FileOpenDialog111
        {
        }
    }
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}

	[Fact]
	public async Task TestInterfaceDeclarationDoesNotStartWithIWithConflictAsync()
	{
		string testCode = @"
public interface Foo
{
}

public interface IFoo { }";
		string fixedCode = @"
public interface IFoo1
{
}

public interface IFoo { }";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(2, 18);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestInterfaceDeclarationDoesNotStartWithIWithMemberMatchingTargetNameAsync()
	{
		string testCode = @"
public interface Foo
{
    int IFoo { get; }
}";
		string fixedCode = @"
public interface IFoo
{
    int IFoo { get; }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(2, 18);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestNestedInterfaceDeclarationDoesNotStartWithIWithConflictAsync()
	{
		string testCode = @"
public class Outer
{
    public interface Foo
    {
    }

    public interface IFoo { }
}";
		string fixedCode = @"
public class Outer
{
    public interface IFoo1
    {
    }

    public interface IFoo { }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(4, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestNestedInterfaceDeclarationDoesNotStartWithIWithContainingTypeConflictAsync()
	{
		string testCode = @"
public class IFoo
{
    public interface Foo
    {
    }
}";
		string fixedCode = @"
public class IFoo
{
    public interface IFoo1
    {
    }
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(4, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestNestedInterfaceDeclarationDoesNotStartWithIWithNonInterfaceConflictAsync()
	{
		string testCode = @"
public class Outer
{
    public interface Foo
    {
    }

    private int IFoo => 0;
}";
		string fixedCode = @"
public class Outer
{
    public interface IFoo1
    {
    }

    private int IFoo => 0;
}";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(4, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}

	[Fact]
	public async Task TestInterfaceDeclarationDoesNotStartWithIWithConflictInAnotherAssemblyAsync()
	{
		string testCode = @"
namespace System
{
    public interface Disposable
    {
    }
}
";
		string fixedCode = @"
namespace System
{
    public interface IDisposable1
    {
    }
}
";

		DiagnosticResult expected = ExpectDiagnostic().WithLocation(4, 22);
		await VerifyCSharpDiagnosticAndFixAsync(testCode, expected, fixedCode);
	}
}
