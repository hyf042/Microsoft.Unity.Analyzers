using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.Unity.Analyzers.StyleCop;

internal class UnityHelper
{
	internal static bool CheckIsUnityMessage(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method)
	{
		if (method?.Body == null)
		{
			return false;
		}

		var classDeclaration = method.FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration == null)
		{
			return false;
		}

		var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
		if (typeSymbol == null)
		{
			return false;
		}

		var scriptInfo = new ScriptInfo(typeSymbol);
		if (!scriptInfo.HasMessages)
		{
			return false;
		}

		var symbol = context.SemanticModel.GetDeclaredSymbol(method);
		if (symbol == null)
		{
			return false;
		}

		return scriptInfo.IsMessage(symbol);
	}

	internal static bool CheckIsUnitySerializeField(SyntaxNodeAnalysisContext context, FieldDeclarationSyntax field)
	{
		var model = context.SemanticModel;
		if (model == null)
		{
			return false;
		}

		ISymbol? symbol;
		switch (context.Node)
		{
			case PropertyDeclarationSyntax pdec:
				symbol = model.GetDeclaredSymbol(pdec);
				break;
			case FieldDeclarationSyntax fdec:
				if (fdec.Declaration.Variables.Count == 0)
				{
					return false;
				}

				// attributes are applied to all fields declaration symbols
				// just get the first one
				symbol = model.GetDeclaredSymbol(fdec.Declaration.Variables[0]);
				break;
			default:
				// we only support field/property analysis
				return false;
		}

		if (symbol == null)
		{
			return false;
		}

		// Check if the containing type is a UnityEngine.Object
		if (!symbol.ContainingType.Extends(typeof(UnityEngine.Object)) && !symbol.ContainingSymbol.GetAttributes().Any(a => a.AttributeClass != null && a.AttributeClass.Matches(typeof(System.SerializableAttribute))))
		{
			return false;
		}

		// Check if the field is marked with [SerializeField] or [SerializeReference]
		if (!symbol.GetAttributes().Any(a => a.AttributeClass != null && (a.AttributeClass.Matches(typeof(UnityEngine.SerializeField)) || a.AttributeClass.Matches(typeof(UnityEngine.SerializeReference)))))
		{
			return false;
		}

		return true;
	}
}
