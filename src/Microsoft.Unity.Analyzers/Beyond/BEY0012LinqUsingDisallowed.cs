#nullable disable

using System.Collections.Immutable;
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

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0012LinqUsingDisallowedAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0012";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0012LinqUsingDisallowedDiagnosticTitle,
		messageFormat: Strings.BEY0012LinqUsingDisallowedDiagnosticMessageFormat,
		category: DiagnosticCategory.Correctness,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0012LinqUsingDisallowedDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterSyntaxNodeAction(HandleCompilationUnit, SyntaxKind.CompilationUnit);
		context.RegisterSyntaxNodeAction(HandleBaseNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
	}

	private static void HandleCompilationUnit(SyntaxNodeAnalysisContext context)
	{
		var compilationUnit = (CompilationUnitSyntax)context.Node;
		var usings = compilationUnit.Usings;
		ProcessUsingsAndReportDiagnostic(usings, context);
	}

	private static void HandleBaseNamespaceDeclaration(SyntaxNodeAnalysisContext context)
	{
		var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;
		var usings = namespaceDeclaration.Usings;
		ProcessUsingsAndReportDiagnostic(usings, context);
	}

	private static void ProcessUsingsAndReportDiagnostic(SyntaxList<UsingDirectiveSyntax> usings, SyntaxNodeAnalysisContext context)
	{
		for (var i = 0; i < usings.Count; i++)
		{
			var usingDirective = usings[i];

			if (usingDirective.Name.ToNormalizedString().Contains("System.Linq"))
			{
				context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), usingDirective.Name.ToNormalizedString()));
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
