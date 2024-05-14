/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Unity.Analyzers.Tests;

public class BeyondNamingRulesTests : BaseCodeFixVerifierTest<BeyondNamingRulesAnalyzer, BeyondNamingRulesCodeFix>
{
	[Fact]
	public async Task Test()
	{
		const string test = @"
using UnityEngine;

class Camera : MonoBehaviour
{
}
";

		await VerifyCSharpDiagnosticAsync(test);

		const string fixedTest = @"
using UnityEngine;

class Camera : MonoBehaviour
{
}
";

		await VerifyCSharpFixAsync(test, fixedTest);
	}
}
