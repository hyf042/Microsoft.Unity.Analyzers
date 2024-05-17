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
public class BEY0008PrivateMethodNamesMustBeginWithUnderscoreAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0008";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0008PrivateMethodNamesMustBeginWithUnderscoreDiagnosticTitle,
		messageFormat: Strings.BEY0008PrivateMethodNamesMustBeginWithUnderscoreDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0008PrivateMethodNamesMustBeginWithUnderscoreDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> MethodDeclarationAction = HandleMethodDeclaration;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		
		context.RegisterSyntaxNodeAction(MethodDeclarationAction, SyntaxKind.MethodDeclaration);
	}

	private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
	{
		var methodDeclaration = (MethodDeclarationSyntax)context.Node;
		if (methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
		{
			// don't analyze an overridden method.
			return;
		}
		if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration).DeclaredAccessibility != Accessibility.Private)
		{
			// don't analyze non-private method.
			return;
		}
		if (UnityHelper.CheckIsUnityMessage(context, methodDeclaration))
		{
			// don't analyze unity message.
			return;
		}

		CheckElementNameToken(context, methodDeclaration.Identifier);
	}

	private static void CheckElementNameToken(SyntaxNodeAnalysisContext context, SyntaxToken identifier)
	{
		if (identifier.IsMissing)
		{
			return;
		}

		if (string.IsNullOrEmpty(identifier.ValueText))
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

		do
		{
			if (identifier.ValueText[0] != '_')
			{
				// Report if not start with underscore
				break;
			}

			string stripPrefix = identifier.ValueText.TrimStart('_');
			if (string.IsNullOrEmpty(stripPrefix))
			{
				// Report if all characters are underscore
				break;
			}

			if (!char.IsUpper(stripPrefix[0]) && !char.IsDigit(stripPrefix[0]))
			{
				// Report the first non-underscore character is not upper case letter
				break;
			}

			// dont report otherwise
			return;
		} while (false);

		context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0008PrivateMethodNamesMustBeginWithUnderscoreCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0008PrivateMethodNamesMustBeginWithUnderscoreAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var document = context.Document;
		var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (var diagnostic in context.Diagnostics)
		{
			var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
			var originalName = token.ValueText;
			if (string.IsNullOrEmpty(originalName))
			{
				continue;
			}

			// Append prefix underscore if not present
			originalName = RenameHelper.AppendPrefixUnderscore(originalName);

			var baseName = originalName.TrimStart('_');
			if (baseName.Length == 0)
			{
				// only offer a code fix if the name does not consist of only underscores.
				continue;
			}

			baseName = char.ToUpper(baseName[0]) + baseName.Substring(1);
			int underscoreCount = originalName.Length - baseName.Length;

			var memberSyntax = RenameHelper.GetParentDeclaration(token);

			SemanticModel semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			var declaredSymbol = semanticModel.GetDeclaredSymbol(memberSyntax);
			if (declaredSymbol == null)
			{
				continue;
			}

			// preserve the underscores for methods.
			var prefix = originalName.Substring(0, underscoreCount);
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
					nameof(BEY0008PrivateMethodNamesMustBeginWithUnderscoreAnalyzer) + "_" + underscoreCount + "_" + index),
				diagnostic);
		}
	}
}
