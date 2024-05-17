#nullable disable

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Unity.Analyzers.Resources;
using Microsoft.Unity.Analyzers.StyleCop;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0011";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreDiagnosticTitle,
		messageFormat: Strings.BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> FieldDeclarationAction = HandleFieldDeclaration;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		
		context.RegisterSyntaxNodeAction(FieldDeclarationAction, SyntaxKind.FieldDeclaration);
	}

	private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
	{
		FieldDeclarationSyntax syntax = (FieldDeclarationSyntax)context.Node;
		if (NamedTypeHelpers.IsContainedInNativeMethodsClass(syntax))
		{
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.ConstKeyword))
		{
			// this diagnostic does not apply to constant fields
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// this diagnostic does not apply to static fields
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
			&& syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// this diagnostic does not apply to static readonly fields
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.PublicKeyword)
			|| syntax.Modifiers.Any(SyntaxKind.InternalKeyword))
		{
			// this diagnostic does not apply to public or internal fields
			return;
		}

		if (!UnityHelper.CheckIsUnitySerializeField(context, syntax))
		{
			// this diagnostic only applies to unity serialized fields
			return;
		}

		var variables = syntax.Declaration?.Variables;
		if (variables == null)
		{
			return;
		}

		foreach (VariableDeclaratorSyntax variableDeclarator in variables.Value)
		{
			if (variableDeclarator == null)
			{
				continue;
			}

			var identifier = variableDeclarator.Identifier;
			if (identifier.IsMissing)
			{
				continue;
			}

			string name = identifier.ValueText;
			if (string.IsNullOrEmpty(name))
			{
				continue;
			}

			if (!name.StartsWith("_", StringComparison.Ordinal))
			{
				// Field names should begin with '_'
				context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), name));
			}
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0011NonPublicUnitySerializeFieldNamesMustBeginWithUnderscoreAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (var diagnostic in context.Diagnostics)
		{
			await CreateChangedSolutionAsync(context, context.Document, diagnostic, context.CancellationToken);
		}

		await Task.CompletedTask;
	}

	private static async Task CreateChangedSolutionAsync(CodeFixContext context, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

		string baseName = RenameHelper.MakeFirstNotUnderscoreLower(
			RenameHelper.AppendPrefixUnderscore(token.ValueText), 1 /* skip leading underscore */);
		var index = 0;
		var newName = baseName;

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var declaredSymbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);
		while (!await RenameHelper.IsValidNewMemberNameAsync(semanticModel, declaredSymbol, newName, cancellationToken).ConfigureAwait(false))
		{
			index++;
			newName = baseName + index;
		}

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(Strings.RenameToCodeFix, newName),
				cancellationToken => RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken),
				nameof(BEY0007InterfaceNamesMustBeginWithIAnalyzer)),
			diagnostic);
	}
}
