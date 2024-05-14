/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Unity.Analyzers.Resources;
using Microsoft.Unity.Analyzers.StyleCop;

namespace Microsoft.Unity.Analyzers;

/// <summary>
/// The code contains a tab or space character which is not consistent with the current project settings.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondUseTabsCorrectlyAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0003";

	internal const string BehaviorKey = "Behavior";
	internal const string ConvertToTabsBehavior = "ConvertToTabs";
	internal const string ConvertToSpacesBehavior = "ConvertToSpaces";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondUseTabsCorrectlyDiagnosticTitle,
		messageFormat: Strings.BeyondUseTabsCorrectlyDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondUseTabsCorrectlyDiagnosticDescription);

	private static readonly Action<SyntaxTreeAnalysisContext> SyntaxTreeAction = HandleSyntaxTree;

	private static readonly ImmutableDictionary<string, string> ConvertToTabsProperties =
		ImmutableDictionary.Create<string, string>().SetItem(BehaviorKey, ConvertToTabsBehavior);

	private static readonly ImmutableDictionary<string, string> ConvertToSpacesProperties =
		ImmutableDictionary.Create<string, string>().SetItem(BehaviorKey, ConvertToSpacesBehavior);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxTreeAction(SyntaxTreeAction);
		});
	}

	private static void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
	{
		SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
		ImmutableArray<TextSpan> excludedSpans;
		if (!LocateExcludedSpans(root, out excludedSpans))
		{
			return;
		}

		ReportIncorrectTabUsage(context, BeyondSettings.IndentationSettings, excludedSpans);
	}

	private static void ReportIncorrectTabUsage(SyntaxTreeAnalysisContext context, IndentationSettings indentationSettings, ImmutableArray<TextSpan> excludedSpans)
	{
		SyntaxTree syntaxTree = context.Tree;
		SourceText sourceText = syntaxTree.GetText(context.CancellationToken);

		string completeText = sourceText.ToString();
		int length = completeText.Length;

		bool useTabs = indentationSettings.UseTabs;
		int tabSize = indentationSettings.TabSize;

		int excludedSpanIndex = 0;
		var lastExcludedSpan = new TextSpan(completeText.Length, 0);
		TextSpan nextExcludedSpan = !excludedSpans.IsEmpty ? excludedSpans[0] : lastExcludedSpan;

		int lastLineStart = 0;
		for (int startIndex = 0; startIndex < length; startIndex++)
		{
			if (startIndex == nextExcludedSpan.Start)
			{
				startIndex = nextExcludedSpan.End - 1;
				excludedSpanIndex++;
				nextExcludedSpan = excludedSpanIndex < excludedSpans.Length ? excludedSpans[excludedSpanIndex] : lastExcludedSpan;
				continue;
			}

			int tabCount = 0;
			bool containsSpaces = false;
			bool tabAfterSpace = false;
			switch (completeText[startIndex])
			{
				case ' ':
					containsSpaces = true;
					break;

				case '\t':
					tabCount++;
					break;

				case '\r':
				case '\n':
					// Handle newlines. We can ignore CR/LF/CRLF issues because we are only tracking column position
					// in a line, and not the line numbers themselves.
					lastLineStart = startIndex + 1;
					continue;

				default:
					continue;
			}

			int endIndex;
			for (endIndex = startIndex + 1; endIndex < length; endIndex++)
			{
				if (endIndex == nextExcludedSpan.Start)
				{
					break;
				}

				if (completeText[endIndex] == ' ')
				{
					containsSpaces = true;
				}
				else if (completeText[endIndex] == '\t')
				{
					tabCount++;
					tabAfterSpace = containsSpaces;
				}
				else
				{
					break;
				}
			}

			if (useTabs && startIndex == lastLineStart)
			{
				// For the case we care about in the following condition (tabAfterSpace is false), spaceCount is
				// the number of consecutive trailing spaces.
				int spaceCount = (endIndex - startIndex) - tabCount;
				if (tabAfterSpace || spaceCount >= tabSize)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Rule,
							Location.Create(syntaxTree, TextSpan.FromBounds(startIndex, endIndex)),
							ConvertToTabsProperties));
				}
			}
			else
			{
				if (tabCount > 0)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Rule,
							Location.Create(syntaxTree, TextSpan.FromBounds(startIndex, endIndex)),
							ConvertToSpacesProperties));
				}
			}

			// Make sure to not analyze overlapping spans
			startIndex = endIndex - 1;
		}
	}

	private static bool LocateExcludedSpans(SyntaxNode root, out ImmutableArray<TextSpan> excludedSpans)
	{
		ImmutableArray<TextSpan>.Builder builder = ImmutableArray.CreateBuilder<TextSpan>();

		// Locate disabled text
		foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
		{
			if (trivia.IsKind(SyntaxKind.DisabledTextTrivia))
			{
				builder.Add(trivia.Span);
			}
			else if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
			{
				if (trivia.ToString().StartsWith("////"))
				{
					// Exclude comments starting with //// because they could contain commented code which contains
					// string or character literals, and we don't want to change the contents of those strings.
					builder.Add(trivia.Span);
				}
			}
		}

		// Locate string literals
		foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
		{
			switch (token.Kind())
			{
				case SyntaxKind.XmlTextLiteralToken:
					if (token.Parent.IsKind(SyntaxKind.XmlCDataSection))
					{
						builder.Add(token.Span);
					}

					break;

				case SyntaxKind.CharacterLiteralToken:
				case SyntaxKind.StringLiteralToken:
				case SyntaxKind.InterpolatedStringTextToken:
					builder.Add(token.Span);
					break;

				default:
					break;
			}
		}

		// Sort the results
		builder.Sort();

		// Combine adjacent and overlapping spans
		ReduceTextSpans(builder);

		excludedSpans = builder.ToImmutable();
		return true;
	}

	private static void ReduceTextSpans(ImmutableArray<TextSpan>.Builder sortedTextSpans)
	{
		if (sortedTextSpans.Count == 0)
		{
			return;
		}

		int currentIndex = 0;
		for (int nextIndex = 1; nextIndex < sortedTextSpans.Count; nextIndex++)
		{
			TextSpan current = sortedTextSpans[currentIndex];
			TextSpan next = sortedTextSpans[nextIndex];
			if (current.End < next.Start)
			{
				// Increment currentIndex this iteration
				currentIndex++;
				sortedTextSpans[currentIndex] = next;
				continue;
			}

			// Since sortedTextSpans is sorted, we already know current and next overlap
			sortedTextSpans[currentIndex] = TextSpan.FromBounds(current.Start, next.End);
		}

		sortedTextSpans.Count = currentIndex + 1;
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondUseTabsCorrectlyCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondUseTabsCorrectlyAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (Diagnostic diagnostic in context.Diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BeyondUseTabsCorrectlyCodeFixTitle,
					cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
					nameof(BeyondUseTabsCorrectlyCodeFix)),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
		SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		return document.WithText(sourceText.WithChanges(FixDiagnostic(BeyondSettings.IndentationSettings, sourceText, diagnostic)));
	}

	private static TextChange FixDiagnostic(IndentationSettings indentationSettings, SourceText sourceText, Diagnostic diagnostic)
	{
		TextSpan span = diagnostic.Location.SourceSpan;

		TextLine startLine = sourceText.Lines.GetLineFromPosition(span.Start);

		bool useTabs = false;
		string behavior;
		if (diagnostic.Properties.TryGetValue(BeyondUseTabsCorrectlyAnalyzer.BehaviorKey, out behavior))
		{
			useTabs = behavior == BeyondUseTabsCorrectlyAnalyzer.ConvertToTabsBehavior;
		}

		string text = sourceText.ToString(TextSpan.FromBounds(startLine.Start, span.End));
		StringBuilder replacement = new StringBuilder();
		int spaceCount = 0;
		int column = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '\t')
			{
				var offsetWithinTabColumn = column % indentationSettings.TabSize;
				var tabWidth = indentationSettings.TabSize - offsetWithinTabColumn;

				if (i >= span.Start - startLine.Start)
				{
					if (useTabs)
					{
						replacement.Length -= spaceCount;
						replacement.Append('\t');
						spaceCount = 0;
					}
					else
					{
						replacement.Append(' ', tabWidth);
					}
				}

				column += tabWidth;
			}
			else
			{
				if (i >= span.Start - startLine.Start)
				{
					replacement.Append(c);
					if (c == ' ')
					{
						spaceCount++;
						if (useTabs)
						{
							// Note that we account for column not yet being incremented
							var offsetWithinTabColumn = (column + 1) % indentationSettings.TabSize;
							if (offsetWithinTabColumn == 0)
							{
								// We reached a tab stop.
								replacement.Length -= spaceCount;
								replacement.Append('\t');
								spaceCount = 0;
							}
						}
					}
					else
					{
						spaceCount = 0;
					}
				}

				if (c == '\r' || c == '\n')
				{
					// Handle newlines. We can ignore CR/LF/CRLF issues because we are only tracking column position
					// in a line, and not the line numbers themselves.
					column = 0;
					spaceCount = 0;
				}
				else
				{
					column++;
				}
			}
		}

		return new TextChange(span, replacement.ToString());
	}
}
