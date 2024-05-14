/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

using System;
using System.Collections.Immutable;
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
public class BeyondElementMustNamedLowerCamelCaseAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0004";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondElementMustNamedLowerCamelCaseDiagnosticTitle,
		messageFormat: Strings.BeyondElementMustNamedLowerCamelCaseDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondElementMustNamedLowerCamelCaseDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> FieldDeclarationAction = HandleFieldDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> EventDeclarationAction = HandleEventDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> EventFieldDeclarationAction = HandleEventFieldDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> PropertyDeclarationAction = HandlePropertyDeclaration;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxNodeAction(FieldDeclarationAction, SyntaxKind.FieldDeclaration);
			context.RegisterSyntaxNodeAction(EventDeclarationAction, SyntaxKind.EventDeclaration);
			context.RegisterSyntaxNodeAction(EventFieldDeclarationAction, SyntaxKind.EventFieldDeclaration);
			context.RegisterSyntaxNodeAction(PropertyDeclarationAction, SyntaxKind.PropertyDeclaration);
		});
	}

	private static void HandleEventDeclaration(SyntaxNodeAnalysisContext context)
	{
		var eventDeclaration = (EventDeclarationSyntax)context.Node;
		if (eventDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
		{
			// Don't analyze an overridden event.
			return;
		}

		CheckElementNameToken(context, eventDeclaration.Identifier);
	}

	private static void HandleEventFieldDeclaration(SyntaxNodeAnalysisContext context)
	{
		EventFieldDeclarationSyntax eventFieldDeclarationSyntax = (EventFieldDeclarationSyntax)context.Node;
		VariableDeclarationSyntax variableDeclarationSyntax = eventFieldDeclarationSyntax.Declaration;
		if (variableDeclarationSyntax == null || variableDeclarationSyntax.IsMissing)
		{
			return;
		}

		foreach (var declarator in variableDeclarationSyntax.Variables)
		{
			if (declarator == null || declarator.IsMissing)
			{
				continue;
			}

			CheckElementNameToken(context, declarator.Identifier);
		}
	}

	private static void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
	{
		var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
		if (propertyDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
		{
			// Don't analyze an overridden property.
			return;
		}

		CheckElementNameToken(context, propertyDeclaration.Identifier);
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

		if (syntax.Modifiers.Any(SyntaxKind.PrivateKeyword)
			|| syntax.Modifiers.Any(SyntaxKind.ProtectedKeyword))
		{
			// this diagnostic does not apply to private or protected fields
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
			CheckElementNameToken(context, identifier);
		}
	}

	private static void CheckElementNameToken(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
	{
		if (identifier.IsMissing)
		{
			return;
		}

		string name = identifier.ValueText;
		if (string.IsNullOrEmpty(name))
		{
			return;
		}

		var index = 0;
		while ((index < name.Length) && name[index] == '_')
		{
			index++;
		}

		if (index == name.Length)
		{
			// ignore fields with all underscores
			return;
		}

		if (char.IsLower(name, index))
		{
			return;
		}

		// Field names should begin with lower-case letter
		context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), name));
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondElementMustNamedLowerCamelCaseCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondElementMustNamedLowerCamelCaseAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var document = context.Document;
		var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (var diagnostic in context.Diagnostics)
		{
			var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
			if (string.IsNullOrEmpty(token.ValueText))
			{
				continue;
			}

			var originalName = token.ValueText;

			var baseName = originalName.TrimStart('_');
			if (baseName.Length == 0)
			{
				// only offer a code fix if the name does not consist of only underscores.
				continue;
			}

			baseName = char.ToLower(baseName[0]) + baseName.Substring(1);
			int underscoreCount = originalName.Length - baseName.Length;

			var memberSyntax = RenameHelper.GetParentDeclaration(token);

			SemanticModel semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			var declaredSymbol = semanticModel.GetDeclaredSymbol(memberSyntax);
			if (declaredSymbol == null)
			{
				continue;
			}

			// preserve the underscores, but only for fields.
			var prefix = declaredSymbol.Kind == SymbolKind.Field ? originalName.Substring(0, underscoreCount) : string.Empty;
			var newName = prefix + baseName;

			int index = 0;
			while (!await RenameHelper.IsValidNewMemberNameAsync(semanticModel, declaredSymbol, newName, context.CancellationToken).ConfigureAwait(false))
			{
				index++;
				newName = prefix + baseName + index;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					string.Format(Strings.RenameToCodeFix, newName),
					cancellationToken => RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken),
					nameof(BeyondElementMustNamedLowerCamelCaseCodeFix) + "_" + underscoreCount + "_" + index),
				diagnostic);
		}
	}
}
