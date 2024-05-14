#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Unity.Analyzers.Resources;
using Microsoft.Unity.Analyzers.StyleCop;

namespace Microsoft.Unity.Analyzers;

/// <summary>
/// The opening or closing brace within a C# statement, element, or expression is not placed on its own line.
/// </summary>
/// <remarks>
/// <para>A violation of this rule occurs when the opening or closing brace within a statement, element, or
/// expression is not placed on its own line. For example:</para>
///
/// <code language="cs">
/// public object Method()
/// {
///   lock (this) {
///     return this.value;
///   }
/// }
/// </code>
///
/// <para>When StyleCop checks this code, a violation of this rule will occur because the opening brace of the lock
/// statement is placed on the same line as the lock keyword, rather than being placed on its own line, as
/// follows:</para>
///
/// <code language="cs">
/// public object Method()
/// {
///   lock (this)
///   {
///     return this.value;
///   }
/// }
/// </code>
///
/// <para>A violation will also occur if the closing brace shares a line with other code. For example:</para>
///
/// <code language="cs">
/// public object Method()
/// {
///   lock (this)
///   {
///     return this.value; }
/// }
/// </code>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondBracesForMultiLineStatementsMustNotShareLineAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0002";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondBracesForMultiLineStatementsMustNotShareLineDiagnosticTitle,
		messageFormat: Strings.BeyondBracesForMultiLineStatementsMustNotShareLineDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondBracesForMultiLineStatementsMustNotShareLineDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> NamespaceDeclarationAction = HandleNamespaceDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> BaseTypeDeclarationAction = HandleBaseTypeDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> AccessorListAction = HandleAccessorList;
	private static readonly Action<SyntaxNodeAnalysisContext> BlockAction = HandleBlock;
	private static readonly Action<SyntaxNodeAnalysisContext> SwitchStatementAction = HandleSwitchStatement;
	private static readonly Action<SyntaxNodeAnalysisContext> InitializerExpressionAction = HandleInitializerExpression;
	private static readonly Action<SyntaxNodeAnalysisContext> AnonymousObjectCreationExpressionAction = HandleAnonymousObjectCreationExpression;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxNodeAction(NamespaceDeclarationAction, SyntaxKind.NamespaceDeclaration);
			context.RegisterSyntaxNodeAction(BaseTypeDeclarationAction, SyntaxKinds.BaseTypeDeclaration);
			context.RegisterSyntaxNodeAction(AccessorListAction, SyntaxKind.AccessorList);
			context.RegisterSyntaxNodeAction(BlockAction, SyntaxKind.Block);
			context.RegisterSyntaxNodeAction(SwitchStatementAction, SyntaxKind.SwitchStatement);
			context.RegisterSyntaxNodeAction(InitializerExpressionAction, SyntaxKinds.InitializerExpression);
			context.RegisterSyntaxNodeAction(AnonymousObjectCreationExpressionAction, SyntaxKind.AnonymousObjectCreationExpression);
		});
	}

	private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
	{
		var syntax = (NamespaceDeclarationSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleBaseTypeDeclaration(SyntaxNodeAnalysisContext context)
	{
		var syntax = (BaseTypeDeclarationSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleAccessorList(SyntaxNodeAnalysisContext context)
	{
		var syntax = (AccessorListSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleBlock(SyntaxNodeAnalysisContext context)
	{
		var syntax = (BlockSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleSwitchStatement(SyntaxNodeAnalysisContext context)
	{
		var syntax = (SwitchStatementSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleInitializerExpression(SyntaxNodeAnalysisContext context)
	{
		var syntax = (InitializerExpressionSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void HandleAnonymousObjectCreationExpression(SyntaxNodeAnalysisContext context)
	{
		var syntax = (AnonymousObjectCreationExpressionSyntax)context.Node;
		CheckBraces(context, syntax.OpenBraceToken, syntax.CloseBraceToken);
	}

	private static void CheckBraces(SyntaxNodeAnalysisContext context, SyntaxToken openBraceToken, SyntaxToken closeBraceToken)
	{
		if (openBraceToken.IsKind(SyntaxKind.None) || closeBraceToken.IsKind(SyntaxKind.None))
		{
			return;
		}

		bool checkCloseBrace = true;
		int openBraceTokenLine = openBraceToken.GetLine();

		if (openBraceTokenLine == closeBraceToken.GetLine())
		{
			if (context.Node.IsKind(SyntaxKind.ArrayInitializerExpression))
			{
				switch (context.Node.Parent.Kind())
				{
					case SyntaxKind.EqualsValueClause:
						if (((EqualsValueClauseSyntax)context.Node.Parent).EqualsToken.GetLine() == openBraceTokenLine)
						{
							return;
						}

						break;

					case SyntaxKind.ArrayCreationExpression:
						if (((ArrayCreationExpressionSyntax)context.Node.Parent).NewKeyword.GetLine() == openBraceTokenLine)
						{
							return;
						}

						break;

					case SyntaxKind.ImplicitArrayCreationExpression:
						if (((ImplicitArrayCreationExpressionSyntax)context.Node.Parent).NewKeyword.GetLine() == openBraceTokenLine)
						{
							return;
						}

						break;

					case SyntaxKind.StackAllocArrayCreationExpression:
						if (((StackAllocArrayCreationExpressionSyntax)context.Node.Parent).StackAllocKeyword.GetLine() == openBraceTokenLine)
						{
							return;
						}

						break;

					case SyntaxKind.ImplicitStackAllocArrayCreationExpression:
						if (((ImplicitStackAllocArrayCreationExpressionSyntax)context.Node.Parent).StackAllocKeyword.GetLine() == openBraceTokenLine)
						{
							return;
						}

						break;

					case SyntaxKind.ArrayInitializerExpression:
						if (!InitializerExpressionSharesLine((InitializerExpressionSyntax)context.Node))
						{
							return;
						}

						checkCloseBrace = false;
						break;

					default:
						break;
				}
			}
			else
			{
				switch (context.Node.Parent.Kind())
				{
					case SyntaxKind.GetAccessorDeclaration:
					case SyntaxKind.SetAccessorDeclaration:
					case SyntaxKind.InitAccessorDeclaration:
					case SyntaxKind.AddAccessorDeclaration:
					case SyntaxKind.RemoveAccessorDeclaration:
					case SyntaxKind.UnknownAccessorDeclaration:
						if (((AccessorDeclarationSyntax)context.Node.Parent).Keyword.GetLine() == openBraceTokenLine)
						{
							// reported as SA1504, if at all (we don't import SA1504 so it's ok for BEYOND)
							return;
						}

						checkCloseBrace = false;
						break;

					default:
						// reported by SA1501 or SA1502 (we don't import SA1501 and SA1502 so it's ok for BEYOND)
						return;
				}
			}
		}

		CheckBraceToken(context, openBraceToken);
		if (checkCloseBrace)
		{
			CheckBraceToken(context, closeBraceToken, openBraceToken);
		}
	}

	private static bool InitializerExpressionSharesLine(InitializerExpressionSyntax node)
	{
		var parent = (InitializerExpressionSyntax)node.Parent;
		var index = parent.Expressions.IndexOf(node);

		return (index > 0) && (parent.Expressions[index - 1].GetEndLine() == parent.Expressions[index].GetLine());
	}

	private static void CheckBraceToken(SyntaxNodeAnalysisContext context, SyntaxToken token, SyntaxToken openBraceToken = default)
	{	
		if (token.IsMissing)
		{
			return;
		}

		int line = token.GetLineSpan().StartLinePosition.Line;

		SyntaxToken previousToken = token.GetPreviousToken(includeZeroWidth: true);
		if (!previousToken.IsMissing)
		{
			if (previousToken.GetLineSpan().StartLinePosition.Line == line)
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation()));

				// no need to report more than one instance for this token
				return;
			}
		}

		SyntaxToken nextToken = token.GetNextToken(includeZeroWidth: true);
		if (!nextToken.IsMissing)
		{
			switch (nextToken.Kind())
			{
				case SyntaxKind.CloseParenToken:
				case SyntaxKind.CommaToken:
				case SyntaxKind.SemicolonToken:
				case SyntaxKind.DotToken:
					// these are allowed to appear on the same line
					return;

				case SyntaxKind.EndOfFileToken:
					// last token of this file
					return;

				case SyntaxKind.WhileKeyword:
					// Because the default Visual Studio code completion snippet for a do-while loop
					// places the while expression on the same line as the closing brace, some users
					// may want to allow that and not have BEY0002 report it as a style error.
					if (BeyondSettings.AllowDoWhileOnClosingBrace)
					{
						if (openBraceToken.Parent.IsKind(SyntaxKind.Block)
							&& openBraceToken.Parent.Parent.IsKind(SyntaxKind.DoStatement))
						{
							return;
						}
					}

					break;

				default:
					break;
			}

			if (nextToken.GetLineSpan().StartLinePosition.Line == line)
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation()));
			}
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondBracesForMultiLineStatementsMustNotShareLineCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondBracesForMultiLineStatementsMustNotShareLineAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (Diagnostic diagnostic in context.Diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BeyondBracesForMultiLineStatementsMustNotShareLineCodeFixTitle,
					cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken),
					nameof(BeyondBracesForMultiLineStatementsMustNotShareLineCodeFix)),
				diagnostic);
		}

		return Task.CompletedTask;
	}

	private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

		var braceToken = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
		var tokenReplacements = GenerateBraceFixes(ImmutableArray.Create(braceToken));

		var newSyntaxRoot = syntaxRoot.ReplaceTokens(tokenReplacements.Keys, (originalToken, rewrittenToken) => tokenReplacements[originalToken]);
		return document.WithSyntaxRoot(newSyntaxRoot);
	}

	private static Dictionary<SyntaxToken, SyntaxToken> GenerateBraceFixes(ImmutableArray<SyntaxToken> braceTokens)
	{
		var tokenReplacements = new Dictionary<SyntaxToken, SyntaxToken>();

		foreach (var braceToken in braceTokens)
		{
			var braceLine = LocationHelpers.GetLineSpan(braceToken).StartLinePosition.Line;
			var braceReplacementToken = braceToken;

			var indentationSteps = DetermineIndentationSteps(braceToken);

			var previousToken = braceToken.GetPreviousToken();

			if (IsAccessorWithSingleLineBlock(previousToken, braceToken))
			{
				var newTrailingTrivia = previousToken.TrailingTrivia
					.WithoutTrailingWhitespace()
					.Add(SyntaxFactory.Space);

				AddReplacement(tokenReplacements, previousToken, previousToken.WithTrailingTrivia(newTrailingTrivia));

				braceReplacementToken = braceReplacementToken.WithLeadingTrivia(braceToken.LeadingTrivia.WithoutLeadingWhitespace());
			}
			else
			{
				// Check if we need to apply a fix before the brace
				if (LocationHelpers.GetLineSpan(previousToken).StartLinePosition.Line == braceLine)
				{
					if (!braceTokens.Contains(previousToken))
					{
						var sharedTrivia = braceReplacementToken.LeadingTrivia.WithoutTrailingWhitespace();
						var previousTokenNewTrailingTrivia = previousToken.TrailingTrivia
						.WithoutTrailingWhitespace()
						.AddRange(sharedTrivia)
						.Add(SyntaxFactory.CarriageReturnLineFeed);

						AddReplacement(tokenReplacements, previousToken, previousToken.WithTrailingTrivia(previousTokenNewTrailingTrivia));
					}

					braceReplacementToken = braceReplacementToken.WithLeadingTrivia(IndentationHelper.GenerateWhitespaceTrivia(BeyondSettings.IndentationSettings, indentationSteps));
				}

				// Check if we need to apply a fix after the brace. No fix is needed when:
				// - The closing brace is followed by a semi-colon or closing paren
				// - The closing brace is the last token in the file
				// - The closing brace is followed by the while expression of a do/while loop and the
				//   allowDoWhileOnClosingBrace setting is enabled.
				var nextToken = braceToken.GetNextToken();
				var nextTokenLine = nextToken.IsKind(SyntaxKind.None) ? -1 : LocationHelpers.GetLineSpan(nextToken).StartLinePosition.Line;
				var isMultiDimensionArrayInitializer = braceToken.IsKind(SyntaxKind.OpenBraceToken) && braceToken.Parent.IsKind(SyntaxKind.ArrayInitializerExpression) && braceToken.Parent.Parent.IsKind(SyntaxKind.ArrayInitializerExpression);
				var allowDoWhileOnClosingBrace = BeyondSettings.AllowDoWhileOnClosingBrace && nextToken.IsKind(SyntaxKind.WhileKeyword) && (braceToken.Parent?.IsKind(SyntaxKind.Block) ?? false) && (braceToken.Parent.Parent?.IsKind(SyntaxKind.DoStatement) ?? false);

				if ((nextTokenLine == braceLine) &&
					(!braceToken.IsKind(SyntaxKind.CloseBraceToken) || !IsValidFollowingToken(nextToken)) &&
					!isMultiDimensionArrayInitializer &&
					!allowDoWhileOnClosingBrace)
				{
					var sharedTrivia = nextToken.LeadingTrivia.WithoutTrailingWhitespace();
					var newTrailingTrivia = braceReplacementToken.TrailingTrivia
						.WithoutTrailingWhitespace()
						.AddRange(sharedTrivia)
						.Add(SyntaxFactory.CarriageReturnLineFeed);

					if (!braceTokens.Contains(nextToken))
					{
						int newIndentationSteps = indentationSteps;
						if (braceToken.IsKind(SyntaxKind.OpenBraceToken))
						{
							newIndentationSteps++;
						}

						if (nextToken.IsKind(SyntaxKind.CloseBraceToken))
						{
							newIndentationSteps = Math.Max(0, newIndentationSteps - 1);
						}

						AddReplacement(tokenReplacements, nextToken, nextToken.WithLeadingTrivia(IndentationHelper.GenerateWhitespaceTrivia(BeyondSettings.IndentationSettings, newIndentationSteps)));
					}

					braceReplacementToken = braceReplacementToken.WithTrailingTrivia(newTrailingTrivia);
				}
			}

			AddReplacement(tokenReplacements, braceToken, braceReplacementToken);
		}

		return tokenReplacements;
	}

	private static bool IsAccessorWithSingleLineBlock(SyntaxToken previousToken, SyntaxToken braceToken)
	{
		if (!braceToken.IsKind(SyntaxKind.OpenBraceToken))
		{
			return false;
		}

		switch (previousToken.Kind())
		{
			case SyntaxKind.GetKeyword:
			case SyntaxKind.SetKeyword:
			case SyntaxKind.InitKeyword:
			case SyntaxKind.AddKeyword:
			case SyntaxKind.RemoveKeyword:
				break;

			default:
				return false;
		}

		var token = braceToken;
		var depth = 1;

		while (depth > 0)
		{
			token = token.GetNextToken();
			switch (token.Kind())
			{
				case SyntaxKind.CloseBraceToken:
					depth--;
					break;

				case SyntaxKind.OpenBraceToken:
					depth++;
					break;
			}
		}

		return LocationHelpers.GetLineSpan(braceToken).StartLinePosition.Line == LocationHelpers.GetLineSpan(token).StartLinePosition.Line;
	}

	private static bool IsValidFollowingToken(SyntaxToken nextToken)
	{
		switch (nextToken.Kind())
		{
			case SyntaxKind.SemicolonToken:
			case SyntaxKind.CloseParenToken:
			case SyntaxKind.CommaToken:
				return true;

			default:
				return false;
		}
	}

	private static int DetermineIndentationSteps(SyntaxToken token)
	{
		// For a closing brace use the indentation of the corresponding opening brace
		if (token.IsKind(SyntaxKind.CloseBraceToken))
		{
			var depth = 1;

			while (depth > 0)
			{
				token = token.GetPreviousToken();
				switch (token.Kind())
				{
					case SyntaxKind.CloseBraceToken:
						depth++;
						break;

					case SyntaxKind.OpenBraceToken:
						depth--;
						break;
				}
			}
		}

		var startLine = GetTokenStartLinePosition(token).Line;

		while (!ContainsStartOfLine(token, startLine))
		{
			token = token.GetPreviousToken();
		}

		return IndentationHelper.GetIndentationSteps(BeyondSettings.IndentationSettings, token);
	}

	private static bool ContainsStartOfLine(SyntaxToken token, int startLine)
	{
		var startLinePosition = GetTokenStartLinePosition(token);

		return (startLinePosition.Line < startLine) || (startLinePosition.Character == 0);
	}

	private static LinePosition GetTokenStartLinePosition(SyntaxToken token)
	{
		return token.SyntaxTree.GetLineSpan(token.FullSpan).StartLinePosition;
	}

	private static void AddReplacement(Dictionary<SyntaxToken, SyntaxToken> tokenReplacements, SyntaxToken originalToken, SyntaxToken replacementToken)
	{
		if (tokenReplacements.ContainsKey(originalToken))
		{
			// This will only happen when a single keyword (like else) has invalid brace tokens before and after it.
			tokenReplacements[originalToken] = tokenReplacements[originalToken].WithTrailingTrivia(replacementToken.TrailingTrivia);
		}
		else
		{
			tokenReplacements[originalToken] = replacementToken;
		}
	}
}
