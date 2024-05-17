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

/// <summary>
/// The name of a C# element does not begin with a lower-case letter.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0004ElementMustNamedLowerCamelCaseAnalyzer : DiagnosticAnalyzer
{
	private const string RuleId = "BEY0004";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0004ElementMustNamedLowerCamelCaseDiagnosticTitle,
		messageFormat: Strings.BEY0004ElementMustNamedLowerCamelCaseDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0004ElementMustNamedLowerCamelCaseDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> FieldDeclarationAction = HandleFieldDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> VariableDeclarationAction = HandleVariableDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> CatchDeclarationAction = HandleCatchDeclaration;
	private static readonly Action<SyntaxNodeAnalysisContext> ForEachStatementAction = HandleForEachStatement;
	private static readonly Action<SyntaxNodeAnalysisContext> SingleVariableDesignationAction = HandleSingleVariableDesignation;
	private static readonly Action<SyntaxNodeAnalysisContext> ParameterAction = HandleParameter;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxNodeAction(FieldDeclarationAction, SyntaxKind.FieldDeclaration);

			context.RegisterSyntaxNodeAction(VariableDeclarationAction, SyntaxKind.VariableDeclaration);
			context.RegisterSyntaxNodeAction(CatchDeclarationAction, SyntaxKind.CatchDeclaration);
			context.RegisterSyntaxNodeAction(ForEachStatementAction, SyntaxKind.ForEachStatement);
			context.RegisterSyntaxNodeAction(SingleVariableDesignationAction, SyntaxKind.SingleVariableDesignation);

			context.RegisterSyntaxNodeAction(ParameterAction, SyntaxKind.Parameter);
		});
	}

	private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
	{
		FieldDeclarationSyntax syntax = (FieldDeclarationSyntax)context.Node;

		if (syntax.Modifiers.Any(SyntaxKind.ConstKeyword))
		{
			// constant fields are handled by BEY0006
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// static fields are handled by BEY0009
			return;
		}

		if (syntax.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
			&& syntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			// static readonly fields are handled by BEY0006
			return;
		}

		if (!syntax.Modifiers.Any(SyntaxKind.PublicKeyword)
			&& !syntax.Modifiers.Any(SyntaxKind.InternalKeyword))
		{
			// this diagnostic does not apply to private (default) or protected fields,
			// they are handled by BEY0010 and BEY0011
			return;
		}

		var declareType = context.SemanticModel.GetTypeInfo(syntax.Declaration?.Type);
		if (declareType.Type?.Extends(typeof(Delegate)) ?? false)
		{
			// this diagnostic does not apply to fields with delegate types
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

	private static void HandleVariableDeclaration(SyntaxNodeAnalysisContext context)
	{
		VariableDeclarationSyntax syntax = (VariableDeclarationSyntax)context.Node;
		if (syntax.Parent.IsKind(SyntaxKind.FieldDeclaration)
			|| syntax.Parent.IsKind(SyntaxKind.EventFieldDeclaration))
		{
			// This diagnostic is only for local variables.
			return;
		}

		LocalDeclarationStatementSyntax parentDeclaration = syntax.Parent as LocalDeclarationStatementSyntax;
		if (parentDeclaration?.IsConst ?? false)
		{
			// this diagnostic does not apply to locals constants
			return;
		}

		foreach (VariableDeclaratorSyntax variableDeclarator in syntax.Variables)
		{
			if (variableDeclarator == null)
			{
				continue;
			}

			var identifier = variableDeclarator.Identifier;
			CheckElementNameToken(context, identifier);
		}
	}

	private static void HandleCatchDeclaration(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((CatchDeclarationSyntax)context.Node).Identifier);
	}

	private static void HandleForEachStatement(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((ForEachStatementSyntax)context.Node).Identifier);
	}

	private static void HandleSingleVariableDesignation(SyntaxNodeAnalysisContext context)
	{
		CheckElementNameToken(context, ((SingleVariableDesignationSyntax)context.Node).Identifier);
	}

	private static void HandleParameter(SyntaxNodeAnalysisContext context)
	{
		ParameterSyntax syntax = (ParameterSyntax)context.Node;
		if (syntax.Parent.Parent.IsKind(SyntaxKind.RecordDeclaration)
			|| syntax.Parent.Parent.IsKind(SyntaxKindEx.RecordStructDeclaration))
		{
			// Positional parameters of a record are treated as properties for naming conventions
			return;
		}

		if (NameMatchesAbstraction(syntax, context.SemanticModel))
		{
			return;
		}

		CheckElementNameToken(context, syntax.Identifier);
	}

	private static bool NameMatchesAbstraction(ParameterSyntax syntax, SemanticModel semanticModel)
	{
		if (!(syntax.Parent is ParameterListSyntax parameterList))
		{
			// This occurs for simple lambda expressions (without parentheses)
			return false;
		}

		var index = parameterList.Parameters.IndexOf(syntax);
		var declaringMember = syntax.Parent.Parent;

		if (!declaringMember.IsKind(SyntaxKind.MethodDeclaration))
		{
			return false;
		}

		var methodDeclaration = (MethodDeclarationSyntax)declaringMember;
		var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

		if (methodSymbol.IsOverride)
		{
			// OverriddenMethod can be null in case of an invalid method declaration -> exit because there is no meaningful analysis to be done.
			if ((methodSymbol.OverriddenMethod == null) || (methodSymbol.OverriddenMethod.Parameters[index].Name == syntax.Identifier.ValueText))
			{
				return true;
			}
		}
		else if (methodSymbol.ExplicitInterfaceImplementations.Length > 0)
		{
			// Checking explicitly implemented interface members here because the code below will not handle them correctly
			foreach (var interfaceMethod in methodSymbol.ExplicitInterfaceImplementations)
			{
				if (interfaceMethod.Parameters[index].Name == syntax.Identifier.ValueText)
				{
					return true;
				}
			}
		}
		else
		{
			var containingType = methodSymbol.ContainingType;
			if (containingType == null)
			{
				return false;
			}

			var implementedInterfaces = containingType.Interfaces;
			foreach (var implementedInterface in implementedInterfaces)
			{
				foreach (var member in implementedInterface.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>())
				{
#pragma warning disable RS1024
					if (methodSymbol.Equals(containingType.FindImplementationForInterfaceMember(member)))
#pragma warning restore RS1024
					{
						return member.Parameters[index].Name == syntax.Identifier.ValueText;
					}
				}
			}
		}

		return false;
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

		if (NamedTypeHelpers.IsContainedInNativeMethodsClass(context.Node))
		{
			// don't analyze elements in NativeMethods classes
			return;
		}

		var symbolInfo = context.SemanticModel.GetDeclaredSymbol(identifier.Parent);
		if (symbolInfo != null && NamedTypeHelpers.IsImplementingAnInterfaceMember(symbolInfo))
		{
			// dont analyze interface implementations (method, property or event)
			return;
		}
		if (symbolInfo != null && NamedTypeHelpers.IsContainedInAttributeDerivedClass(symbolInfo))
		{
			// dont analyze elements in classes derived from Attribute
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

		// Element names should begin with lower-case letter
		context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), name));
	}
}

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0004ElementMustNamedLowerCamelCaseCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0004ElementMustNamedLowerCamelCaseAnalyzer.Rule.Id);

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

			string prefix;
			switch (declaredSymbol.Kind)
			{
				case SymbolKind.Field:
				case SymbolKind.Local:
				case SymbolKind.Parameter:
					// preserve the underscores, but only for fields, variables and parameters.
					prefix = originalName.Substring(0, underscoreCount);
					break;
				default:
					prefix = string.Empty;
					break;
			}
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
					nameof(BEY0004ElementMustNamedLowerCamelCaseCodeFix) + "_" + underscoreCount + "_" + index),
				diagnostic);
		}
	}
}
