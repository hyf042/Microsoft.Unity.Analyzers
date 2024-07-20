#nullable disable

using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Unity.Analyzers.Resources;
using Microsoft.Unity.Analyzers.StyleCop;
using System;

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0012LinqUsingDisallowedAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0012";
	private const string LinqNamespaceSubStr= "System.Linq";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0012LinqUsingDisallowedDiagnosticTitle,
		messageFormat: Strings.BEY0012LinqUsingDisallowedDiagnosticMessageFormat,
		category: DiagnosticCategory.Performance,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: false,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0012LinqUsingDisallowedDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
	}

	private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
	{
		var root = context.Tree.GetRoot(context.CancellationToken);
		var walker = new AllowedMacrosSyntaxWalker(context);
		walker.Visit(root);
	}

	private class AllowedMacrosSyntaxWalker : CSharpSyntaxWalker
	{
		private static readonly string[] AllowedConditionalMacros = new string[] { "UNITY_EDITOR", "BEYOND_DEBUG" };

		private readonly SyntaxTreeAnalysisContext _context;
		private readonly Stack<bool?> _isWithinAllowedMacrosBlockStack = new Stack<bool?>();

		public AllowedMacrosSyntaxWalker(SyntaxTreeAnalysisContext context)
			: base(SyntaxWalkerDepth.StructuredTrivia)
		{
			_context = context;
		}

		public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
		{
			base.VisitIfDirectiveTrivia(node);

			_isWithinAllowedMacrosBlockStack.Push(ContainsInAllowedConditionalMacros(node.Condition));
		}

		public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
		{
			base.VisitElifDirectiveTrivia(node);

			if (_isWithinAllowedMacrosBlockStack.Count > 0)
			{
				_isWithinAllowedMacrosBlockStack.Pop(); // Pop current context
			}

			_isWithinAllowedMacrosBlockStack.Push(ContainsInAllowedConditionalMacros(node.Condition));
		}

		public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
		{
			base.VisitElseDirectiveTrivia(node);

			if (_isWithinAllowedMacrosBlockStack.Count > 0)
			{
				var currentContext = _isWithinAllowedMacrosBlockStack.Pop();
				_isWithinAllowedMacrosBlockStack.Push(!currentContext);
			}
		}

		public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
		{
			base.VisitEndIfDirectiveTrivia(node);

			if (_isWithinAllowedMacrosBlockStack.Count > 0)
			{
				_isWithinAllowedMacrosBlockStack.Pop();
			}
		}

		public override void VisitUsingDirective(UsingDirectiveSyntax node)
		{
			base.VisitUsingDirective(node);

			var usingName = node.Name.ToNormalizedString();
			if (usingName.Contains(LinqNamespaceSubStr) && !CheckIsWithinAllowedConditionalMacros())
			{
				_context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), usingName));
			}
		}

		private bool CheckIsWithinAllowedConditionalMacros()
		{
			bool? result = null;
			foreach (var flag in _isWithinAllowedMacrosBlockStack)
			{
				result = HandleLogicalAND(flag, result);
			}
			return result.HasValue && result.Value;
		}

		private bool? ContainsInAllowedConditionalMacros(ExpressionSyntax expression)
		{
			switch (expression)
			{
				case BinaryExpressionSyntax binaryExpression:
				{
					var lhs = ContainsInAllowedConditionalMacros(binaryExpression.Left);
					var rhs = ContainsInAllowedConditionalMacros(binaryExpression.Right);
					if (binaryExpression.IsKind(SyntaxKind.LogicalAndExpression))
					{
						return HandleLogicalAND(lhs, rhs);
					}
					else
					{
						return HandleLogicalOR(lhs, rhs);
					}
				}
				case PrefixUnaryExpressionSyntax prefixUnaryExpression:
				{
					// Handle logical NOT
					return !ContainsInAllowedConditionalMacros(prefixUnaryExpression.Operand);
				}
				case IdentifierNameSyntax identifierName:
				{
					return Array.IndexOf(AllowedConditionalMacros, identifierName.Identifier.Text) >= 0 ? true : null;
				}
				case LiteralExpressionSyntax literalExpression:
				{
					// Handle literal expression, if it is not true nor false, return null
					if (literalExpression.IsKind(SyntaxKind.TrueLiteralExpression))
					{
						return true;
					}
					else if (literalExpression.IsKind(SyntaxKind.FalseLiteralExpression))
					{
						return false;
					}
					else
					{
						return null;
					}
				}
				case ParenthesizedExpressionSyntax parenthesizedExpression:
				{
					return ContainsInAllowedConditionalMacros(parenthesizedExpression.Expression);
				}
				default:
					return null;
			}
		}

		private bool? HandleLogicalAND(bool? lhs, bool? rhs)
		{
			if (lhs.HasValue && rhs.HasValue)
			{
				return lhs.Value && rhs.Value;
			}
			else if (lhs.HasValue)
			{
				return lhs.Value;
			}
			else if (rhs.HasValue)
			{
				return rhs.Value;
			}
			else
			{
				return null;
			}
		}

		private bool? HandleLogicalOR(bool? lhs, bool? rhs)
		{
			// Handle logical OR
			if (lhs.HasValue && rhs.HasValue)
			{
				return lhs.Value || rhs.Value;
			}
			else if (lhs.HasValue)
			{
				return lhs.Value;
			}
			else if (rhs.HasValue)
			{
				return rhs.Value;
			}
			else
			{
				return null;
			}
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0012LinqUsingDisallowedCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0012LinqUsingDisallowedAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (var diagnostic in context.Diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BEY0012LinqUsingDisallowedCodeFixTitle,
					cancellationToken => DeleteUsingAsync(context.Document, diagnostic, cancellationToken),
					nameof(BEY0012LinqUsingDisallowedAnalyzer)),
				context.Diagnostics);
		}

		await Task.CompletedTask;
	}

	private static async Task<Document> DeleteUsingAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<UsingDirectiveSyntax>();

		var newRoot = syntaxRoot?.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
		if (newRoot == null)
		{
			return document;
		}

		return document.WithSyntaxRoot(newRoot);
	}
}
