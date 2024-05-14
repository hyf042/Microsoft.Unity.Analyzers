/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Unity.Analyzers.Resources;
using Microsoft.Unity.Analyzers.StyleCop;

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondNamingRulesAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0010";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondNamingRulesDiagnosticTitle,
		messageFormat: Strings.BeyondNamingRulesDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondNamingRulesDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	private static readonly System.Action<SyntaxNodeAnalysisContext> ClassDeclarationAction = HandleClassDeclaration;

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxNodeAction(ClassDeclarationAction, SyntaxKind.ClassDeclaration);
		});
	}

	private static void HandleClassDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckUpperCamelCase(context, ((ClassDeclarationSyntax)context.Node).Identifier);
	}

	private static void CheckUpperCamelCase(SyntaxNodeAnalysisContext context, SyntaxToken identifier, bool allowUnderscoreDigit = false)
	{
		if (identifier.IsMissing)
		{
			return;
		}

		if (string.IsNullOrEmpty(identifier.ValueText))
		{
			return;
		}

		if (!char.IsLower(identifier.ValueText[0]) && identifier.ValueText[0] != '_')
		{
			return;
		}

		if (allowUnderscoreDigit && (identifier.ValueText.Length > 1) && (identifier.ValueText[0] == '_') && char.IsDigit(identifier.ValueText[1]))
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

		context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), symbolInfo.Name));
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondNamingRulesCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondNamingRulesAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		// var declaration = await context.GetFixableNodeAsync<MethodDeclarationSyntax>();
		// if (declaration == null)
		//     return;

		// context.RegisterCodeFix(
		//     CodeAction.Create(
		//         Strings.BeyondNamingRulesCodeFixTitle,
		//         ct => {},
		//         declaration.ToFullString()),
		//     context.Diagnostics);
	}
}
