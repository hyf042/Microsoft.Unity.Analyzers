#nullable disable

using System;
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

/// <summary>
/// The name of a C# element does not name with all capital letters and underscores.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0006ElementMustNamedAllCapsAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0006";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0006ElementMustNamedAllCapsDiagnosticTitle,
		messageFormat: Strings.BEY0006ElementMustNamedAllCapsDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0006ElementMustNamedAllCapsDiagnosticDescription);

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
		if (!CheckFieldDiagnostic(context, syntax))
		{
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

			CheckFieldNameToken(context, variableDeclarator);
		}
	}

	private static bool CheckFieldDiagnostic(SyntaxNodeAnalysisContext context, FieldDeclarationSyntax field)
	{
		if (field.Modifiers.Any(SyntaxKind.ConstKeyword))
		{
			// this diagnostic does apply to constant fields
			return true;
		}

		if (field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) && field.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// If it's a static readonly field, check whether the field type is value type or not.
			// Since we don't diagnostic reference type.
			// **NOTICE** we consider string as value type here since it's immutable.
			var typeSymbol = context.SemanticModel.GetTypeInfo(field.Declaration?.Type).Type;
			return typeSymbol.IsValueType || typeSymbol.Extends(typeof(string));
		}

		return false;
	}

	private static void CheckFieldNameToken(SyntaxNodeAnalysisContext context, VariableDeclaratorSyntax variable)
	{
		var identifier = variable.Identifier;
		if (identifier.IsMissing)
		{
			return;
		}

		if (NamedTypeHelpers.IsContainedInNativeMethodsClass(context.Node))
		{
			return;
		}

		var symbolInfo = context.SemanticModel.GetDeclaredSymbol(identifier.Parent);
		if (symbolInfo != null && NamedTypeHelpers.IsImplementingAnInterfaceMember(symbolInfo))
		{
			return;
		}

		string name = identifier.ValueText;
		if (CheckNameValid(name))
		{
			return;
		}

		// Field names should be ALL_CAPS
		context.ReportDiagnostic(Diagnostic.Create(Rule, variable.GetLocation(), identifier.ValueText));
	}

	private static bool CheckNameValid(string fieldName)
	{
		if (string.IsNullOrEmpty(fieldName))
		{
			return true;
		}

		foreach (var c in fieldName)
		{
			if (char.IsLower(c))
			{
				return false;
			}
		}

		return true;
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0006ElementMustNamedAllCapsCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0006ElementMustNamedAllCapsAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var document = context.Document;
		var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (var diagnostic in context.Diagnostics)
		{
			await CreateChangedSolutionAsync(context, document, diagnostic, context.CancellationToken).ConfigureAwait(false);
		}

		await Task.CompletedTask;
	}

	private static async Task CreateChangedSolutionAsync(CodeFixContext context, Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

		string baseName = token.ValueText.ToUpper();
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
