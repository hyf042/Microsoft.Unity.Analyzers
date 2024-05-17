using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

/// <summary>
/// Unit tests for <see cref="BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreAnalyzer"/>.
/// </summary>
public class BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreTests : BaseCodeFixVerifierTest<BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreAnalyzer, BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreCodeFix>
{
	protected override string[] DisabledDiagnostics
	{
		get =>
		[
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
	[InlineData("public static readonly")]
	[InlineData("private static readonly")]
	[InlineData("internal static readonly")]
	[InlineData("protected internal static readonly")]
	[InlineData("protected static readonly")]
	public async Task TestThatDiagnosticIsNotReported_NotUnityObjectAsync(string modifiers)
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo
{{
[SerializeField]
{0}
string Bar = """", car = """", Dar = """";
[SerializeReference]
{0}
List<int> keys = default;
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Theory]
	[InlineData("public readonly")]
	[InlineData("internal readonly")]
	[InlineData("protected internal readonly")]
	[InlineData("public")]
	[InlineData("internal")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsNotReported_PublicOrInternalAsync(string modifiers)
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{{
[SerializeField]
{0}
string Bar = """", car = """", Dar = """";
[SerializeReference]
{0}
List<int> keys = default;
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(testCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("readonly")]
	[InlineData("private readonly")]
	[InlineData("protected readonly")]
	public async Task TestThatDiagnosticIsReported_SingleFieldAsync(string modifiers)
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{{
[SerializeField]
{0}
string bar;
[SerializeField]
{0}
string _car;
[SerializeField]
{0}
string Dar;
[SerializeReference]
{0}
List<int> m_Ear;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("bar").WithLocation(9, 8),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(15, 8),
			ExpectDiagnostic().WithArguments("m_Ear").WithLocation(18, 11),
		};

		var fixedCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{{
[SerializeField]
{0}
string _bar;
[SerializeField]
{0}
string _car;
[SerializeField]
{0}
string _dar;
[SerializeReference]
{0}
List<int> _ear;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("readonly")]
	[InlineData("private readonly")]
	[InlineData("protected readonly")]
	public async Task TestThatDiagnosticIsReported_SerializableAsync(string modifiers)
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Foo
{{
[SerializeField]
{0}
string bar;
[SerializeField]
{0}
string _car;
[SerializeField]
{0}
string Dar;
[SerializeReference]
{0}
List<int> m_Ear;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("bar").WithLocation(10, 8),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(16, 8),
			ExpectDiagnostic().WithArguments("m_Ear").WithLocation(19, 11),
		};

		var fixedCode = @"
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Foo
{{
[SerializeField]
{0}
string _bar;
[SerializeField]
{0}
string _car;
[SerializeField]
{0}
string _dar;
[SerializeReference]
{0}
List<int> _ear;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("readonly")]
	[InlineData("private readonly")]
	[InlineData("protected readonly")]
	public async Task TestThatDiagnosticIsReported_MultipleFieldsAsync(string modifiers)
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{{
[SerializeField]
{0}
string Bar, car, Dar;
[SerializeReference]
{0}
List<int> keys = default;
}}";

		DiagnosticResult[] expected =
		{
			ExpectDiagnostic().WithArguments("Bar").WithLocation(9, 8),
			ExpectDiagnostic().WithArguments("car").WithLocation(9, 13),
			ExpectDiagnostic().WithArguments("Dar").WithLocation(9, 18),
			ExpectDiagnostic().WithArguments("keys").WithLocation(12, 11),
		};

		var fixedCode = @"
using UnityEngine;
using System.Collections.Generic;

public class Foo : MonoBehaviour
{{
[SerializeField]
{0}
string _bar, _car, _dar;
[SerializeReference]
{0}
List<int> _keys = default;
}}";

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(testCode, modifiers), expected, string.Format(fixedCode, modifiers));
	}

	[Fact]
	public async Task TestFieldPlacedInsideNativeMethodsClassAsync()
	{
		var testCode = @"
using UnityEngine;
using System.Collections.Generic;

public class FooNativeMethods : MonoBehaviour
{
[SerializeField]
private string Bar, car, Dar;
[SerializeReference]
private List<int> _keys = default;
}";

		await VerifyCSharpDiagnosticAsync(testCode);
	}
}
