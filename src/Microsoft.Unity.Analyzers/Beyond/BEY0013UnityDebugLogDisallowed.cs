#nullable disable

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.Unity.Analyzers.Resources;

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0013UnityDebugLogDisallowedAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0013";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0013UnityDebugLogDisallowedDiagnosticTitle,
		messageFormat: Strings.BEY0013UnityDebugLogDisallowedDiagnosticMessageFormat,
		category: DiagnosticCategory.Performance,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: false,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0013UnityDebugLogDisallowedDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterSyntaxNodeAction(HandleInvocationExpression, SyntaxKind.InvocationExpression);
	}

	private static void HandleInvocationExpression(SyntaxNodeAnalysisContext context)
	{
		var invocationExpr = (InvocationExpressionSyntax)context.Node;

		var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
		if (memberAccessExpr == null)
		{
			return;
		}

		var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpr);
		var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
		if (methodSymbol == null)
		{
			return;
		}

		if (methodSymbol.ContainingType.ToString() == "UnityEngine.Debug" && methodSymbol.Name.StartsWith("Log"))
		{
			var diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation(), methodSymbol.ToDisplayString());
			context.ReportDiagnostic(diagnostic);
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0013UnityDebugLogDisallowedCodeFix : CodeFixProvider
{
	private const string FormatSuffix = "Format";
	private const string DLoggerName = "DLogger";

	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0013UnityDebugLogDisallowedAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (var diagnostic in context.Diagnostics)
		{
			var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
								 .FirstAncestorOrSelf<InvocationExpressionSyntax>();

			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BEY0013UnityDebugLogDisallowedCodeFixTitle,
					cancellationToken => ReplaceWithDLoggerAsync(context.Document, node, cancellationToken),
					nameof(BEY0012LinqUsingDisallowedAnalyzer)),
				context.Diagnostics);
		}

		await Task.CompletedTask;
	}

	private static async Task<Document> ReplaceWithDLoggerAsync(
		Document document, InvocationExpressionSyntax node, CancellationToken cancellationToken)
	{
		var memberAccessExpr = (MemberAccessExpressionSyntax)node.Expression;

		// New method name will strip suffix 'Format' since DLogger no longer needs that
		var methodName = memberAccessExpr.Name.Identifier.Text;
		var newMethodName =
			methodName.EndsWith(FormatSuffix) ?
			methodName.Substring(0, methodName.Length - FormatSuffix.Length) :
			methodName;

		var newMemberAccess = memberAccessExpr
			.WithName(SyntaxFactory.IdentifierName(newMethodName))
			.WithExpression(SyntaxFactory.IdentifierName(DLoggerName));

		var arguments = node.ArgumentList.Arguments;
		var newArguments = SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments);

		int interpolatedIndex = newArguments.IndexOf(a => a.Expression is InterpolatedStringExpressionSyntax);
		if (interpolatedIndex >= 0)
		{
			var interpolatedString = newArguments[interpolatedIndex].Expression as InterpolatedStringExpressionSyntax;
			var interpolationExpressions = interpolatedString.Contents
				.OfType<InterpolationSyntax>()
				.Select(interpolation => interpolation.Expression);

			int index = 0;
			StringBuilder sb = new();
			foreach (var content in interpolatedString.Contents)
			{
				if (content is InterpolationSyntax)
				{
					sb.Append('{');
					sb.Append(index++);
					sb.Append('}');
				}
				else if (content is InterpolatedStringTextSyntax text)
				{
					sb.Append(text.TextToken.ValueText);
				}
				else
				{
					sb.Append(content.ToFullString());
				}
			}
			newArguments = newArguments.Replace(
				newArguments[interpolatedIndex],
				SyntaxFactory.Argument(
					SyntaxFactory.LiteralExpression(
						SyntaxKind.StringLiteralExpression,
						SyntaxFactory.Literal(sb.ToString()))));
			newArguments = newArguments.InsertRange(
				interpolatedIndex + 1, interpolationExpressions.Select(e => SyntaxFactory.Argument(e)));
		}

		var newInvocation = node
			.WithExpression(newMemberAccess)
			.WithArgumentList(SyntaxFactory.ArgumentList(newArguments))
			.WithLeadingTrivia(node.GetLeadingTrivia())
			.WithTrailingTrivia(node.GetTrailingTrivia());

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		editor.ReplaceNode(node, newInvocation);

		var newRoot = editor.GetChangedRoot();
		return document.WithSyntaxRoot(newRoot);
	}
}
