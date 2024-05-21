/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Microsoft.Unity.Analyzers.StyleCop;

/// <summary>
/// **NOTICE** this class is copied from https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/StyleCop.Analyzers/StyleCop.Analyzers.CodeFixes/Helpers/RenameHelper.cs
/// </summary>
internal static class RenameHelper
{
	public static async Task<Solution> RenameSymbolAsync(Document document, SyntaxNode root, SyntaxToken declarationToken, string newName, CancellationToken cancellationToken)
	{
		var annotatedRoot = root.ReplaceToken(declarationToken, declarationToken.WithAdditionalAnnotations(RenameAnnotation.Create()));
		var annotatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, annotatedRoot);
		var annotatedDocument = annotatedSolution.GetDocument(document.Id);

		annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var annotatedToken = annotatedRoot.FindToken(declarationToken.SpanStart);

		var semanticModel = await annotatedDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var symbol = semanticModel.GetDeclaredSymbol(annotatedToken.Parent, cancellationToken);

		var newSolution = await Renamer.RenameSymbolAsync(annotatedSolution, symbol, newName, null, cancellationToken).ConfigureAwait(false);

		// TODO: return annotatedSolution instead of newSolution if newSolution contains any new errors (for any project)
		return newSolution;
	}

	public static async Task<bool> IsValidNewMemberNameAsync(SemanticModel semanticModel, ISymbol symbol, string name, CancellationToken cancellationToken)
	{
		if (symbol.Kind == SymbolKind.NamedType)
		{
			TypeKind typeKind = ((INamedTypeSymbol)symbol).TypeKind;

			// If the symbol is a class or struct, the name can't be the same as any of its members.
			if (typeKind == TypeKind.Class || typeKind == TypeKind.Struct)
			{
				var members = (symbol as INamedTypeSymbol)?.GetMembers(name);
				if (members.HasValue && !members.Value.IsDefaultOrEmpty)
				{
					return false;
				}
			}
		}

		var containingSymbol = symbol.ContainingSymbol;

		if (symbol.Kind == SymbolKind.TypeParameter)
		{
			// If the symbol is a type parameter, the name can't be the same as any type parameters of the containing type.
			if (containingSymbol?.ContainingSymbol is INamedTypeSymbol parentSymbol
				&& parentSymbol.TypeParameters.Any(t => t.Name == name))
			{
				return false;
			}

			// Move up one level for the next validation step.
			containingSymbol = containingSymbol?.ContainingSymbol;
		}

		if (containingSymbol is INamespaceOrTypeSymbol containingNamespaceOrTypeSymbol)
		{
			if (containingNamespaceOrTypeSymbol.Kind == SymbolKind.Namespace)
			{
				// Make sure to use the compilation namespace so interfaces in referenced assemblies are considered
				containingNamespaceOrTypeSymbol = semanticModel.Compilation.GetCompilationNamespace((INamespaceSymbol)containingNamespaceOrTypeSymbol);
			}
			else if (containingNamespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
			{
				TypeKind typeKind = ((INamedTypeSymbol)containingNamespaceOrTypeSymbol).TypeKind;

				// If the containing type is a class or struct, the name can't be the same as the name of the containing
				// type.
				if ((typeKind == TypeKind.Class || typeKind == TypeKind.Struct)
					&& containingNamespaceOrTypeSymbol.Name == name)
				{
					return false;
				}
			}

			// The name can't be the same as the name of an other member of the same type. At this point no special
			// consideration is given to overloaded methods.
			ImmutableArray<ISymbol> siblings = containingNamespaceOrTypeSymbol.GetMembers(name);
			if (!siblings.IsDefaultOrEmpty)
			{
				return false;
			}

			return true;
		}
		else if (containingSymbol.Kind == SymbolKind.Method)
		{
			IMethodSymbol methodSymbol = (IMethodSymbol)containingSymbol;
			if (methodSymbol.Parameters.Any(i => i.Name == name)
				|| methodSymbol.TypeParameters.Any(i => i.Name == name))
			{
				return false;
			}

			IMethodSymbol outermostMethod = methodSymbol;
			while (outermostMethod.ContainingSymbol.Kind == SymbolKind.Method)
			{
				outermostMethod = (IMethodSymbol)outermostMethod.ContainingSymbol;
				if (outermostMethod.Parameters.Any(i => i.Name == name)
					|| outermostMethod.TypeParameters.Any(i => i.Name == name))
				{
					return false;
				}
			}

			foreach (var syntaxReference in outermostMethod.DeclaringSyntaxReferences)
			{
				SyntaxNode syntaxNode = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
				LocalNameFinder localNameFinder = new LocalNameFinder(name);
				localNameFinder.Visit(syntaxNode);
				if (localNameFinder.Found)
				{
					return false;
				}
			}

			return true;
		}
		else
		{
			return true;
		}
	}

	public static SyntaxNode GetParentDeclaration(SyntaxToken token)
	{
		SyntaxNode parent = token.Parent;

		while (parent != null)
		{
			switch (parent.Kind())
			{
				case SyntaxKind.VariableDeclarator:
				case SyntaxKind.Parameter:
				case SyntaxKind.TypeParameter:
				case SyntaxKind.CatchDeclaration:
				case SyntaxKind.ExternAliasDirective:
				case SyntaxKind.QueryContinuation:
				case SyntaxKind.FromClause:
				case SyntaxKind.LetClause:
				case SyntaxKind.JoinClause:
				case SyntaxKind.JoinIntoClause:
				case SyntaxKind.ForEachStatement:
				case SyntaxKind.UsingDirective:
				case SyntaxKind.LabeledStatement:
				case SyntaxKind.AnonymousObjectMemberDeclarator:
				case SyntaxKind.LocalFunctionStatement:
				case SyntaxKind.SingleVariableDesignation:
					return parent;

				default:
					if (parent is MemberDeclarationSyntax declarationParent)
					{
						return declarationParent;
					}

					break;
			}

			parent = parent.Parent;
		}

		return null;
	}

	public static string AppendPrefixUnderscore(string originName)
	{
		char prefixLetter = '_';
		if (string.IsNullOrEmpty(originName))
		{
			return $"{prefixLetter}";
		}

		if (originName.StartsWith("m_") || originName.StartsWith("s_"))
		{
			return originName.Substring(1);
		}
		else if (originName[0] == prefixLetter)
		{
			return originName;
		}
		else
		{
			return $"{prefixLetter}{originName}";
		}
	}

	public static string AppendPrefixLetterWithUnderscore(string originName, char prefixLetter)
	{
		string prefix = $"{prefixLetter}_";
		if (string.IsNullOrEmpty(originName))
		{
			return prefix;
		}

		if (originName.StartsWith(prefix))
		{
			return originName;
		}
		else if (originName.Length > 1 && originName[1] == '_'
				&& (originName[0] == '_' || originName[0] == 'm' || originName[0] == 's'))
		{
			// if originName starts with "__" or "s_" or "m_", replace the first '_' with prefixLetter
			return $"{prefixLetter}{originName.Substring(1)}";
		}
		else if (originName[0] == '_')
		{
			return $"{prefixLetter}{originName}";
		}
		else
		{
			return $"{prefix}{originName}";
		}
	}

	public static string MakeFirstNotUnderscoreUpper(string originName, int startIndex)
	{
		int index = FirstIndexOfNotSpecificChar(originName, '_', startIndex);
		if (index < 0)
		{
			return originName;
		}
		return $"{originName.Substring(0, index)}{char.ToUpper(originName[index])}{originName.Substring(index + 1)}";
	}

	public static string MakeFirstNotUnderscoreLower(string originName, int startIndex)
	{
		int index = FirstIndexOfNotSpecificChar(originName, '_', startIndex);
		if (index < 0)
		{
			return originName;
		}
		return $"{originName.Substring(0, index)}{char.ToLower(originName[index])}{originName.Substring(index + 1)}";
	}

	private static int FirstIndexOfNotSpecificChar(string str, char c, int startIndex)
	{
		int index = startIndex;
		while (index < str.Length)
		{
			if (str[index] != c)
			{
				return index;
			}
			index++;
		}
		return -1;
	}

	private class LocalNameFinder : CSharpSyntaxWalker
	{
		private readonly string name;

		public LocalNameFinder(string name)
		{
			this.name = name;
		}

		public bool Found
		{
			get;
			private set;
		}

		public override void Visit(SyntaxNode node)
		{
			switch (node.Kind())
			{
				case SyntaxKind.LocalFunctionStatement:
					this.Found |= ((LocalFunctionStatementSyntax)node).Identifier.ValueText == this.name;
					break;

				case SyntaxKind.SingleVariableDesignation:
					this.Found |= ((SingleVariableDesignationSyntax)node).Identifier.ValueText == this.name;
					break;

				default:
					break;
			}

			base.Visit(node);
		}

		public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitVariableDeclarator(node);
		}

		public override void VisitParameter(ParameterSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitParameter(node);
		}

		public override void VisitTypeParameter(TypeParameterSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitTypeParameter(node);
		}

		public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitCatchDeclaration(node);
		}

		public override void VisitQueryContinuation(QueryContinuationSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitQueryContinuation(node);
		}

		public override void VisitFromClause(FromClauseSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitFromClause(node);
		}

		public override void VisitLetClause(LetClauseSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitLetClause(node);
		}

		public override void VisitJoinClause(JoinClauseSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitJoinClause(node);
		}

		public override void VisitJoinIntoClause(JoinIntoClauseSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitJoinIntoClause(node);
		}

		public override void VisitForEachStatement(ForEachStatementSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitForEachStatement(node);
		}

		public override void VisitLabeledStatement(LabeledStatementSyntax node)
		{
			this.Found |= node.Identifier.ValueText == this.name;
			base.VisitLabeledStatement(node);
		}

		public override void VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
		{
			this.Found |= node.NameEquals?.Name?.Identifier.ValueText == this.name;
			base.VisitAnonymousObjectMemberDeclarator(node);
		}
	}
}
