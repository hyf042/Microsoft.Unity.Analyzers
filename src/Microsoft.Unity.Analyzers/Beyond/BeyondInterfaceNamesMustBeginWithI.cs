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

/// <summary>
/// Implements a code fix for <see cref="SA1302InterfaceNamesMustBeginWithI"/>.
/// </summary>
/// <remarks>
/// <para>To fix a violation of this rule, add the capital letter I to the front of the interface name, or place the
/// item within a <c>NativeMethods</c> class if appropriate.</para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BeyondInterfaceNamesMustBeginWithIAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0009";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BeyondInterfaceNamesMustBeginWithIDiagnosticTitle,
		messageFormat: Strings.BeyondInterfaceNamesMustBeginWithIDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BeyondInterfaceNamesMustBeginWithIDiagnosticDescription);

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
			context.ReportDiagnostic(Diagnostic.Create(Rule, interfaceDeclaration.Identifier.GetLocation()));
		}
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BeyondInterfaceNamesMustBeginWithICodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BeyondInterfaceNamesMustBeginWithIAnalyzer.Rule.Id);

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

		return await RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken).ConfigureAwait(false);
	}
}
