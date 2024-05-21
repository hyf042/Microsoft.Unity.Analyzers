using System.Collections.Immutable;
using Microsoft.Unity.Analyzers.StyleCop;

namespace Microsoft.Unity.Analyzers
{
	/// <summary>
	/// StyleCop has its own customizable settings like global.json.
	/// However, in Beyond, we don't need to customize it, just hard-code it here.
	/// </summary>
	internal static class BeyondSettings
	{
		internal const bool AllowDoWhileOnClosingBrace = true;
		internal readonly static IndentationSettings IndentationSettings =
			new IndentationSettings(4 /* indentationSize */, 4 /* tabSize */, false /* useTabs */);
		internal readonly static ImmutableArray<string> AllowedNamespaceComponents = ImmutableArray<string>.Empty;
		internal readonly static ImmutableArray<string> AllowedMethodPrefixes = ImmutableArray<string>.Empty;
		internal readonly static ImmutableArray<string> AllowedVariablePrefixes = ImmutableArray.Create("UI");
	}
}
