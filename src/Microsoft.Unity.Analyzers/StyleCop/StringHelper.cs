using System.Collections.Immutable;

namespace Microsoft.Unity.Analyzers.StyleCop;

internal class StringHelper
{
	public static string AppendPrefixUnderscore(string originName)
	{
		char prefixLetter = '_';
		if (string.IsNullOrEmpty(originName))
		{
			return $"{prefixLetter}";
		}

		if (originName.StartsWith("m_") || originName.StartsWith("s_"))
		{
			return originName.Substring(1);
		}
		else if (originName[0] == prefixLetter)
		{
			return originName;
		}
		else
		{
			return $"{prefixLetter}{originName}";
		}
	}

	public static string AppendPrefixLetterWithUnderscore(string originName, char prefixLetter)
	{
		string prefix = $"{prefixLetter}_";
		if (string.IsNullOrEmpty(originName))
		{
			return prefix;
		}

		if (originName.StartsWith(prefix))
		{
			return originName;
		}
		else if (originName.Length > 1 && originName[1] == '_'
				&& (originName[0] == '_' || originName[0] == 'm' || originName[0] == 's'))
		{
			// if originName starts with "__" or "s_" or "m_", replace the first '_' with prefixLetter
			return $"{prefixLetter}{originName.Substring(1)}";
		}
		else if (originName[0] == '_')
		{
			return $"{prefixLetter}{originName}";
		}
		else
		{
			return $"{prefix}{originName}";
		}
	}

	public static string MakeFirstNotUnderscoreUpper(string originName, int startIndex)
	{
		int index = FirstIndexOfNotSpecificChar(originName, '_', startIndex);
		if (index < 0)
		{
			return originName;
		}
		return $"{originName.Substring(0, index)}{char.ToUpper(originName[index])}{originName.Substring(index + 1)}";
	}

	public static string MakeFirstNotUnderscoreLower(string originName, int startIndex)
	{
		int index = FirstIndexOfNotSpecificChar(originName, '_', startIndex);
		if (index < 0)
		{
			return originName;
		}
		return $"{originName.Substring(0, index)}{char.ToLower(originName[index])}{originName.Substring(index + 1)}";
	}

	public static bool StartsWithIgnorePrefixUnderscore(string str, ImmutableArray<string> prefixs)
	{
		int index = FirstIndexOfNotSpecificChar(str, '_', 0);
		if (index < 0)
		{
			return false;
		}
		str = str.Substring(index);
		foreach (var prefix in prefixs)
		{
			if (str.StartsWith(prefix))
			{
				return true;
			}
		}
		return false;
	}

	private static int FirstIndexOfNotSpecificChar(string str, char c, int startIndex)
	{
		int index = startIndex;
		while (index < str.Length)
		{
			if (str[index] != c)
			{
				return index;
			}
			index++;
		}
		return -1;
	}
}
