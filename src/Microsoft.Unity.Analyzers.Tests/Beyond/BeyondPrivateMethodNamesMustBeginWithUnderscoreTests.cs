/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BeyondPrivateMethodNamesMustBeginWithUnderscoreTests : BaseCodeFixVerifierTest<BeyondPrivateMethodNamesMustBeginWithUnderscoreAnalyzer, BeyondPrivateMethodNamesMustBeginWithUnderscoreCodeFix>
{
	private DiagnosticResult Diagnostic() => ExpectDiagnostic();

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsNotReported_UnityMessage(string modifiers)
	{
		const string test = @"
using UnityEngine;

class Camera : MonoBehaviour
{{
{0}
void Awake() {{}}
{0}
void Start() {{}}
{0}
void OnDestroy() {{}}
{0}
void FixedUpdate() {{}}
{0}
void OnApplicationPause(bool pause) {{}}
}}

class StateMachine : ScriptableObject
{{
{0}
void Awake() {{}}
{0}
void OnDestroy() {{}}
{0}
void OnDisable() {{}}
{0}
void OnEnable() {{}}
{0}
void OnValidate() {{}}
{0}
void Reset() {{}}
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(test, modifiers));
	}

	[Theory]
	[InlineData("public")]
	[InlineData("internal")]
	[InlineData("protected")]
	[InlineData("protected internal")]
	public async Task TestThatDiagnosticIsNotReported_NonPrivateMethod(string modifiers)
	{
		const string test = @"public abstract class BaseBehaviour
{{
    {0} abstract void Bar();
}}

public class Foo : BaseBehaviour
{{
{0}
override void Bar() {{}}
{0}
void Car() {{}}
{0}
virtual void Dar() {{}}
{0}
static void Ear() {{}}
}}";

		await VerifyCSharpDiagnosticAsync(string.Format(test, modifiers));
	}

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	public async Task TestThatDiagnosticIsReported_NormalClass(string modifiers)
	{
		const string test = @"public class Foo
{{
{0}
void Bar() {{}}
{0}
static void Car() {{}}
{0}
void Dar(int a, int b) {{}}
{0}
static void Ear(string message) {{}}
}}";

		const string fixCode = @"public class Foo
{{
{0}
void _Bar() {{}}
{0}
static void _Car() {{}}
{0}
void _Dar(int a, int b) {{}}
{0}
static void _Ear(string message) {{}}
}}";

		DiagnosticResult[] expected =
		{
			Diagnostic().WithArguments("Bar").WithLocation(4, 6),
			Diagnostic().WithArguments("Car").WithLocation(6, 13),
			Diagnostic().WithArguments("Dar").WithLocation(8, 6),
			Diagnostic().WithArguments("Ear").WithLocation(10, 13),
		};

		await VerifyCSharpDiagnosticAndFixAsync(string.Format(test, modifiers), expected, string.Format(fixCode, modifiers));
	}
}
