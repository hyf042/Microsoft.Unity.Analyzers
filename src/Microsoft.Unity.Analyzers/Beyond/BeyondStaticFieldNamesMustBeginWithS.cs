/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

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

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondStaticFieldNamesMustBeginWithSAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0010";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondStaticFieldNamesMustBeginWithSDiagnosticTitle,
		messageFormat: Strings.BeyondStaticFieldNamesMustBeginWithSDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondStaticFieldNamesMustBeginWithSDiagnosticDescription);

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

		if (!syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// this diagnostic does not apply to non static fields
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
		{
			// this diagnostic does not apply to readonly static fields
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

			if (!name.StartsWith("s_", StringComparison.Ordinal))
			{
				// Field names should begin with 's_'
				context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), name));
			}
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondStaticFieldNamesMustBeginWithSCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondStaticFieldNamesMustBeginWithSAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (var diagnostic in context.Diagnostics)
		{
			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BeyondInterfaceNamesMustBeginWithICodeFixTitle,
					cancellationToken => CreateChangedSolutionAsync(context.Document, diagnostic, cancellationToken),
					nameof(BeyondInterfaceNamesMustBeginWithIAnalyzer)),
				diagnostic);
		}

		await Task.CompletedTask;
	}

	private static async Task<Solution> CreateChangedSolutionAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
		string baseName;
		if (token.ValueText.StartsWith("s_"))
		{
			baseName = token.ValueText;
		}
		else if (token.ValueText.StartsWith("_"))
		{
			baseName = "s" + token.ValueText;
		}
		else
		{
			baseName = "s_" + token.ValueText;
		}
		var index = 0;
		var newName = baseName;

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var declaredSymbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);
		while (!await RenameHelper.IsValidNewMemberNameAsync(semanticModel, declaredSymbol, newName, cancellationToken).ConfigureAwait(false))
		{
			index++;
			newName = baseName + index;
		}

		return await RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken).ConfigureAwait(false);
	}
}
