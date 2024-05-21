/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Unity.Analyzers.Resources;
// BEYOND modify begin
using Microsoft.Unity.Analyzers.StyleCop;
// BEYOND modify end

namespace Microsoft.Unity.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EmptyUnityMessageAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "UNT0001";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.EmptyUnityMessageDiagnosticTitle,
		messageFormat: Strings.EmptyUnityMessageDiagnosticMessageFormat,
		category: DiagnosticCategory.Performance,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.EmptyUnityMessageDiagnosticDescription);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
	}

	private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
	{
		var method = context.Node as MethodDeclarationSyntax;
		if (method?.Body == null)
			return;

		if (method.HasPolymorphicModifier())
			return;

		if (method.Body.Statements.Count > 0)
			return;

		// BEYOND modify begin
		if (!UnityHelper.CheckIsUnityMessage(context, method))
			return;

		context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier.ValueText));
		// BEYOND modify end
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class EmptyUnityMessageCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EmptyUnityMessageAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var declaration = await context.GetFixableNodeAsync<MethodDeclarationSyntax>();
		if (declaration == null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				Strings.EmptyUnityMessageCodeFixTitle,
				ct => DeleteEmptyMessageAsync(context.Document, declaration, ct),
				FixableDiagnosticIds.Single()), // using DiagnosticId as equivalence key for BatchFixer
			context.Diagnostics);
	}

	private static async Task<Document> DeleteEmptyMessageAsync(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var newRoot = root?.RemoveNode(declaration, SyntaxRemoveOptions.KeepNoTrivia);
		if (newRoot == null)
			return document;

		return document.WithSyntaxRoot(newRoot);
	}
}
