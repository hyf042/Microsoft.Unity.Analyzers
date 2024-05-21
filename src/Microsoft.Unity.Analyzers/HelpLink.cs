/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

namespace Microsoft.Unity.Analyzers;

internal static class HelpLink
{
	public static string ForDiagnosticId(string ruleId)
	{
		return $"http://perforce.int.hypergryph.com/files/DM42.Beyond.Project/Main/Beyond_Mainline/ExternalTools/Microsoft.Unity.Analyzers/doc/{ruleId}.md";
	}
}
