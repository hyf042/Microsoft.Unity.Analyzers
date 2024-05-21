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
/// The name of a C# interface does not begin with the capital letter I.
/// </summary>
/// <remarks>
/// <para>A violation of this rule occurs when the name of an interface does not begin with the capital letter I.
/// Interface names should always begin with I. For example, <c>ICustomer</c>.</para>
///
/// <para>If the field or variable name is intended to match the name of an item associated with Win32 or COM, and
/// thus cannot begin with the letter I, place the field or variable within a special <c>NativeMethods</c> class. A
/// <c>NativeMethods</c> class is any class which contains a name ending in <c>NativeMethods</c>, and is intended as
/// a placeholder for Win32 or COM wrappers. StyleCop will ignore this violation if the item is placed within a
/// <c>NativeMethods</c> class.</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0007InterfaceNamesMustBeginWithIAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0007";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0007InterfaceNamesMustBeginWithIDiagnosticTitle,
		messageFormat: Strings.BEY0007InterfaceNamesMustBeginWithIDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0007InterfaceNamesMustBeginWithIDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> InterfaceDeclarationAction = HandleInterfaceDeclaration;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterSyntaxNodeAction(InterfaceDeclarationAction, SyntaxKind.InterfaceDeclaration);
	}

	private static void HandleInterfaceDeclaration(SyntaxNodeAnalysisContext context)
	{
		var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;
		if (interfaceDeclaration.Identifier.IsMissing)
		{
			return;
		}

		if (NamedTypeHelpers.IsContainedInNativeMethodsClass(interfaceDeclaration))
		{
			return;
		}

		string name = interfaceDeclaration.Identifier.ValueText;
		if (name != null && !name.StartsWith("I", StringComparison.Ordinal))
		{
			context.ReportDiagnostic(Diagnostic.Create(Rule, interfaceDeclaration.Identifier.GetLocation(), name));
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0007InterfaceNamesMustBeginWithICodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0007InterfaceNamesMustBeginWithIAnalyzer.Rule.Id);

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
		var baseName = "I" + token.ValueText;
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
