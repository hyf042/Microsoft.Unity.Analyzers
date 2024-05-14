/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Unity.Analyzers.Resources;

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondNonPublicFieldNamesMustBeginWithPrefixAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0007";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondNonPublicFieldNamesMustBeginWithPrefixDiagnosticTitle,
		messageFormat: Strings.BeyondNonPublicFieldNamesMustBeginWithPrefixDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondNonPublicFieldNamesMustBeginWithPrefixDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		
		// TODO: context.RegisterSyntaxNodeAction
		// example: context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondNonPublicFieldNamesMustBeginWithPrefixCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondNonPublicFieldNamesMustBeginWithPrefixAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		// var declaration = await context.GetFixableNodeAsync<MethodDeclarationSyntax>();
		// if (declaration == null)
		//     return;

		// context.RegisterCodeFix(
		//     CodeAction.Create(
		//         Strings.BeyondNonPublicFieldNamesMustBeginWithPrefixCodeFixTitle,
		//         ct => {},
		//         declaration.ToFullString()),
		//     context.Diagnostics);
	}
}
