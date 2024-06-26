/*--------------------------------------------------------------------------------------------
 *  Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *-------------------------------------------------------------------------------------------*/

#nullable disable

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Unity.Analyzers.StyleCop;

/// <summary>
/// **NOTICE** this class is copied from https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/StyleCop.Analyzers/StyleCop.Analyzers/Lightup/SyntaxKindEx.cs
/// </summary>
internal static class SyntaxKindEx
{
	public const SyntaxKind ManagedKeyword = (SyntaxKind)8445;
	public const SyntaxKind UnmanagedKeyword = (SyntaxKind)8446;
	public const SyntaxKind RequiredKeyword = (SyntaxKind)8447;
	public const SyntaxKind FileKeyword = (SyntaxKind)8449;
	public const SyntaxKind UnsignedRightShiftExpression = (SyntaxKind)8692;
	public const SyntaxKind UnsignedRightShiftAssignmentExpression = (SyntaxKind)8726;
	public const SyntaxKind FileScopedNamespaceDeclaration = (SyntaxKind)8845;
	public const SyntaxKind SlicePattern = (SyntaxKind)9034;
	public const SyntaxKind ListPattern = (SyntaxKind)9035;
	public const SyntaxKind FunctionPointerParameter = (SyntaxKind)9057;
	public const SyntaxKind FunctionPointerUnmanagedCallingConventionList = (SyntaxKind)9066;
	public const SyntaxKind RecordStructDeclaration = (SyntaxKind)9068;
	public const SyntaxKind CollectionExpression = (SyntaxKind)9076;
}

/// <summary>
/// **NOTICE** this class is copied from https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/StyleCop.Analyzers/StyleCop.Analyzers/Helpers/SyntaxKinds.cs
/// </summary>
internal static class SyntaxKinds
{
	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseNamespaceDeclarationSyntaxWrapper"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseNamespaceDeclarationSyntaxWrapper"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseNamespaceDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.NamespaceDeclaration,
			SyntaxKindEx.FileScopedNamespaceDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseTypeDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseTypeDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.ClassDeclaration,
			SyntaxKind.StructDeclaration,
			SyntaxKind.InterfaceDeclaration,
			SyntaxKind.EnumDeclaration,
			SyntaxKind.RecordDeclaration,
			SyntaxKindEx.RecordStructDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="TypeDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> TypeDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.ClassDeclaration,
			SyntaxKind.StructDeclaration,
			SyntaxKind.InterfaceDeclaration,
			SyntaxKind.RecordDeclaration,
			SyntaxKindEx.RecordStructDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseFieldDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseFieldDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseFieldDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.FieldDeclaration,
			SyntaxKind.EventFieldDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseMethodDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseMethodDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseMethodDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.DestructorDeclaration,
			SyntaxKind.OperatorDeclaration,
			SyntaxKind.ConversionOperatorDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BasePropertyDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BasePropertyDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.EventDeclaration,
			SyntaxKind.IndexerDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="AccessorDeclarationSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="AccessorDeclarationSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> AccessorDeclaration { get; } =
		ImmutableArray.Create(
			SyntaxKind.GetAccessorDeclaration,
			SyntaxKind.SetAccessorDeclaration,
			SyntaxKind.AddAccessorDeclaration,
			SyntaxKind.RemoveAccessorDeclaration,
			SyntaxKind.UnknownAccessorDeclaration);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="InitializerExpressionSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="InitializerExpressionSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> InitializerExpression { get; } =
		ImmutableArray.Create(
			SyntaxKind.ArrayInitializerExpression,
			SyntaxKind.CollectionInitializerExpression,
			SyntaxKind.ComplexElementInitializerExpression,
			SyntaxKind.ObjectInitializerExpression);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="DocumentationCommentTriviaSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="DocumentationCommentTriviaSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> DocumentationComment { get; } =
		ImmutableArray.Create(
			SyntaxKind.SingleLineDocumentationCommentTrivia,
			SyntaxKind.MultiLineDocumentationCommentTrivia);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="ConstructorInitializerSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="ConstructorInitializerSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> ConstructorInitializer { get; } =
		ImmutableArray.Create(
			SyntaxKind.BaseConstructorInitializer,
			SyntaxKind.ThisConstructorInitializer);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="LambdaExpressionSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="LambdaExpressionSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> LambdaExpression { get; } =
		ImmutableArray.Create(
			SyntaxKind.ParenthesizedLambdaExpression,
			SyntaxKind.SimpleLambdaExpression);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="AnonymousFunctionExpressionSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as an
	/// <see cref="AnonymousFunctionExpressionSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> AnonymousFunctionExpression { get; } =
		ImmutableArray.Create(
			SyntaxKind.ParenthesizedLambdaExpression,
			SyntaxKind.SimpleLambdaExpression,
			SyntaxKind.AnonymousMethodExpression);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="SimpleNameSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="SimpleNameSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> SimpleName { get; } =
		ImmutableArray.Create(
			SyntaxKind.GenericName,
			SyntaxKind.IdentifierName);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseParameterListSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseParameterListSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseParameterList { get; } =
		ImmutableArray.Create(
			SyntaxKind.ParameterList,
			SyntaxKind.BracketedParameterList);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseArgumentListSyntax"/>.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which appear in the syntax tree as a
	/// <see cref="BaseArgumentListSyntax"/>.
	/// </value>
	public static ImmutableArray<SyntaxKind> BaseArgumentList { get; } =
		ImmutableArray.Create(
			SyntaxKind.ArgumentList,
			SyntaxKind.BracketedArgumentList);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which represent keywords of integer literals.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which represent keywords of integer literals.
	/// </value>
	public static ImmutableArray<SyntaxKind> IntegerLiteralKeyword { get; } =
		ImmutableArray.Create(
			SyntaxKind.IntKeyword,
			SyntaxKind.LongKeyword,
			SyntaxKind.ULongKeyword,
			SyntaxKind.UIntKeyword);

	/// <summary>
	/// Gets a collection of <see cref="SyntaxKind"/> values which represent keywords of real literals.
	/// </summary>
	/// <value>
	/// A collection of <see cref="SyntaxKind"/> values which represent keywords of real literals.
	/// </value>
	public static ImmutableArray<SyntaxKind> RealLiteralKeyword { get; } =
		ImmutableArray.Create(
			SyntaxKind.FloatKeyword,
			SyntaxKind.DoubleKeyword,
			SyntaxKind.DecimalKeyword);
}
