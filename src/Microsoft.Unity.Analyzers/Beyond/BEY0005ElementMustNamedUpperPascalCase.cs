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
/// The name of a C# element does not begin with an upper-case letter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0005ElementMustNamedUpperPascalCaseAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0005";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0005ElementMustNamedUpperPascalCaseDiagnosticTitle,
		messageFormat: Strings.BEY0005ElementMustNamedUpperPascalCaseDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0005ElementMustNamedUpperPascalCaseDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> BaseNamespaceDeclarationAction = HandleBaseNamespaceDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> ClassDeclarationAction = HandleClassDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> RecordDeclarationAction = HandleRecordDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> EnumDeclarationAction = HandleEnumDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> EnumMemberDeclarationAction = HandleEnumMemberDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> StructDeclarationAction = HandleStructDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> DelegateDeclarationAction = HandleDelegateDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> MethodDeclarationAction = HandleMethodDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> LocalFunctionStatementAction = HandleLocalFunctionStatement;
	private static readonly Action<SyntaxNodeAnalysisContext> ParameterAction = HandleParameter;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			// Note: Interfaces are handled by BEY0007
			// Note: Properties and events are handled by BEY0004
			// Note: Fields are handled by BEY0004, BEY0006, BEY0009, BEY0010, BEY0011
			context.RegisterSyntaxNodeAction(BaseNamespaceDeclarationAction, SyntaxKind.NamespaceDeclaration);
			context.RegisterSyntaxNodeAction(ClassDeclarationAction, SyntaxKind.ClassDeclaration);
			context.RegisterSyntaxNodeAction(RecordDeclarationAction, SyntaxKind.RecordDeclaration);
			context.RegisterSyntaxNodeAction(RecordDeclarationAction, SyntaxKindEx.RecordStructDeclaration);
			context.RegisterSyntaxNodeAction(EnumDeclarationAction, SyntaxKind.EnumDeclaration);
			context.RegisterSyntaxNodeAction(EnumMemberDeclarationAction, SyntaxKind.EnumMemberDeclaration);
			context.RegisterSyntaxNodeAction(StructDeclarationAction, SyntaxKind.StructDeclaration);
			context.RegisterSyntaxNodeAction(DelegateDeclarationAction, SyntaxKind.DelegateDeclaration);
			context.RegisterSyntaxNodeAction(MethodDeclarationAction, SyntaxKind.MethodDeclaration);
			context.RegisterSyntaxNodeAction(LocalFunctionStatementAction, SyntaxKind.LocalFunctionStatement);
			// positional parameters of recordsare treated as properties, only handle this case here
			context.RegisterSyntaxNodeAction(ParameterAction, SyntaxKind.Parameter);
		});
	}

	private static void HandleBaseNamespaceDeclaration(SyntaxNodeAnalysisContext context)
	{
		NameSyntax nameSyntax = ((NamespaceDeclarationSyntax)context.Node).Name;
		CheckNamespaceNameSyntax(context, nameSyntax);
	}

	private static void CheckNamespaceNameSyntax(SyntaxNodeAnalysisContext context, NameSyntax nameSyntax)
	{
		if (nameSyntax == null || nameSyntax.IsMissing)
		{
			return;
		}

		if (nameSyntax is QualifiedNameSyntax qualifiedNameSyntax)
		{
			CheckNamespaceNameSyntax(context, qualifiedNameSyntax.Left);
			CheckNamespaceNameSyntax(context, qualifiedNameSyntax.Right);
			return;
		}

		if (nameSyntax is SimpleNameSyntax simpleNameSyntax &&
			!BeyondSettings.AllowedNamespaceComponents.Contains(simpleNameSyntax.Identifier.ValueText))
		{
			CheckElementNameToken(context, simpleNameSyntax.Identifier);
			return;
		}

		// TODO: any other cases?
	}

	private static void HandleClassDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((ClassDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleRecordDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((TypeDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleEnumDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((EnumDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleEnumMemberDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((EnumMemberDeclarationSyntax)context.Node).Identifier, true);
	}

	private static void HandleStructDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((StructDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleDelegateDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((DelegateDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
	{
		var methodDeclaration = (MethodDeclarationSyntax)context.Node;
		if (methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
		{
			// Don't analyze an overridden method.
			return;
		}

		if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration).DeclaredAccessibility == Accessibility.Private
			|| UnityHelper.CheckIsUnityMessage(context, methodDeclaration))
		{
			// Private Method is handled by BEY0008, don't analyze unity message.
			return;
		}

		if (StringHelper.StartsWithIgnorePrefixUnderscore(
			methodDeclaration.Identifier.ValueText, BeyondSettings.AllowedMethodPrefixes))
		{
			// If prefix exists in BeyondSettings.AllowedMethodPrefixes, don't analyze.
			return;
		}

		CheckElementNameToken(context, methodDeclaration.Identifier);
	}

	private static void HandleLocalFunctionStatement(SyntaxNodeAnalysisContext context)
	{
		var localFunctionStatement = (LocalFunctionStatementSyntax)context.Node;
		CheckElementNameToken(context, localFunctionStatement.Identifier);
	}

	private static void HandleParameter(SyntaxNodeAnalysisContext context)
	{
		var parameterDeclaration = (ParameterSyntax)context.Node;
		if (!parameterDeclaration.Parent.IsKind(SyntaxKind.ParameterList)
			|| (!parameterDeclaration.Parent.Parent.IsKind(SyntaxKind.RecordDeclaration) && !parameterDeclaration.Parent.Parent.IsKind(SyntaxKindEx.RecordStructDeclaration)))
		{
			// Only positional parameters of records are treated as properties
			return;
		}

		CheckElementNameToken(context, parameterDeclaration.Identifier);
	}

	private static void CheckElementNameToken(SyntaxNodeAnalysisContext context, SyntaxToken identifier, bool allowUnderscoreDigit = false)
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

		/* This code uses char.IsLower(...) instead of !char.IsUpper(...) for all of the following reasons:
		 *  1. Foreign languages may not have upper case variants for certain characters
		 *  2. This diagnostic appears targeted for "English" identifiers.
		 *
		 * See DotNetAnalyzers/StyleCopAnalyzers#369 for additional information:
		 * https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/369
		 */
		if (!char.IsLower(identifier.ValueText[0]) && identifier.ValueText[0] != '_')
		{
			return;
		}

		if (allowUnderscoreDigit && (identifier.ValueText.Length > 1) && (identifier.ValueText[0] == '_') && char.IsDigit(identifier.ValueText[1]))
		{
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0005ElementMustNamedUpperPascalCaseCodeFix : CodeFixProvider
{
	/// <summary>
	/// During conflict resolution for fields, this suffix is tried before falling back to 1, 2, 3, etc...
	/// </summary>
	private const string Suffix = "Value";

	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0005ElementMustNamedUpperPascalCaseAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var document = context.Document;
		var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (var diagnostic in context.Diagnostics)
		{
			var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
			var tokenText = token.ValueText.TrimStart('_');
			if (tokenText == string.Empty)
			{
				// Skip this one, since we can't create a new identifier from this
				continue;
			}

			var baseName = char.ToUpper(tokenText[0]) + tokenText.Substring(1);
			var newName = baseName;
			var memberSyntax = RenameHelper.GetParentDeclaration(token);

			if (memberSyntax.IsKind(SyntaxKind.NamespaceDeclaration))
			{
				// namespaces are not symbols. So we are just renaming the namespace
				Task<Document> RenameNamespaceAsync(CancellationToken cancellationToken)
				{
					IdentifierNameSyntax identifierSyntax = (IdentifierNameSyntax)token.Parent;

					var newIdentifierSyntax = identifierSyntax.WithIdentifier(SyntaxFactory.Identifier(newName));

					var newRoot = root.ReplaceNode(identifierSyntax, newIdentifierSyntax);
					return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
				}

				context.RegisterCodeFix(
					CodeAction.Create(
						string.Format(Strings.RenameToCodeFix, newName),
						(Func<CancellationToken, Task<Document>>)RenameNamespaceAsync,
						nameof(BEY0005ElementMustNamedUpperPascalCaseCodeFix) + "_" + diagnostic.Id),
					diagnostic);
			}
			else if (memberSyntax != null)
			{
				SemanticModel semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

				var declaredSymbol = semanticModel.GetDeclaredSymbol(memberSyntax);
				if (declaredSymbol == null)
				{
					continue;
				}

				bool usedSuffix = false;
				if (declaredSymbol.Kind == SymbolKind.Field
					&& declaredSymbol.ContainingType?.TypeKind != TypeKind.Enum
					&& !await RenameHelper.IsValidNewMemberNameAsync(semanticModel, declaredSymbol, newName, context.CancellationToken).ConfigureAwait(false))
				{
					usedSuffix = true;
					newName += Suffix;
				}

				int index = 0;
				while (!await RenameHelper.IsValidNewMemberNameAsync(semanticModel, declaredSymbol, newName, context.CancellationToken).ConfigureAwait(false))
				{
					usedSuffix = false;
					index++;
					newName = baseName + index;
				}

				context.RegisterCodeFix(
					CodeAction.Create(
						string.Format(Strings.RenameToCodeFix, newName),
						cancellationToken => RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken),
						nameof(BEY0005ElementMustNamedUpperPascalCaseCodeFix) + "_" + diagnostic.Id + "_" + usedSuffix + "_" + index),
					diagnostic);
			}
		}
	}
}
