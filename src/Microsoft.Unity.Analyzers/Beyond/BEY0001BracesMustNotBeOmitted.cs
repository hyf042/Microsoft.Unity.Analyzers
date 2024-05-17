#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.Unity.Analyzers.Resources;

namespace Microsoft.Unity.Analyzers;

/// <summary>
/// The opening and closing braces for a C# statement have been omitted.
/// </summary>
/// <remarks>
/// <para>A violation of this rule occurs when the opening and closing braces for a statement have been omitted. In
/// C#, some types of statements may optionally include braces. Examples include <c>if</c>, <c>while</c>, and
/// <c>for</c> statements. For example, an if-statement may be written without braces:</para>
///
/// <code language="csharp">
/// if (true)
///     return this.value;
/// </code>
///
/// <para>Although this is legal in C#, Beyond always requires the braces to be present, to increase the
/// readability and maintainability of the code.</para>
///
/// <para>When the braces are omitted, it is possible to introduce an error in the code by inserting an additional
/// statement beneath the if-statement. For example:</para>
///
/// <code language="csharp">
/// if (true)
///     this.value = 2;
///     return this.value;
/// </code>
///
/// <para>Glancing at this code, it appears as if both the assignment statement and the return statement are
/// children of the if-statement. In fact, this is not true. Only the assignment statement is a child of the
/// if-statement, and the return statement will always execute regardless of the outcome of the if-statement.</para>
///
/// <para>Beyond always requires the opening and closing braces to be present, to prevent these kinds of
/// errors:</para>
///
/// <code language="csharp">
/// if (true)
/// {
///     this.value = 2;
///     return this.value;
/// }
/// </code>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BEY0001BracesMustNotBeOmittedAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// The ID for diagnostics produced by the <see cref="BEY0001BracesMustNotBeOmittedAnalyzer"/> analyzer.
	/// </summary>
	private const string RuleId = "BEY0001";

	internal static readonly DiagnosticDescriptor Rule = new(
		id: RuleId,
		title: Strings.BEY0001BracesMustNotBeOmittedDiagnosticTitle,
		messageFormat: Strings.BEY0001BracesMustNotBeOmittedDiagnosticMessageFormat,
		category: DiagnosticCategory.Maintainability,
		defaultSeverity: DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri: HelpLink.ForDiagnosticId(RuleId),
		description: Strings.BEY0001BracesMustNotBeOmittedDiagnosticDescription);

	private static readonly Action<SyntaxNodeAnalysisContext> IfStatementAction = HandleIfStatement;
	private static readonly Action<SyntaxNodeAnalysisContext> UsingStatementAction = HandleUsingStatement;

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterCompilationStartAction(context =>
		{
			context.RegisterSyntaxNodeAction(IfStatementAction, SyntaxKind.IfStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((DoStatementSyntax)ctx.Node).Statement), SyntaxKind.DoStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((WhileStatementSyntax)ctx.Node).Statement), SyntaxKind.WhileStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((ForStatementSyntax)ctx.Node).Statement), SyntaxKind.ForStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((ForEachStatementSyntax)ctx.Node).Statement), SyntaxKind.ForEachStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((FixedStatementSyntax)ctx.Node).Statement), SyntaxKind.FixedStatement);
			context.RegisterSyntaxNodeAction(UsingStatementAction, SyntaxKind.UsingStatement);
			context.RegisterSyntaxNodeAction(ctx => CheckChildStatement(ctx, ((LockStatementSyntax)ctx.Node).Statement), SyntaxKind.LockStatement);
		});
	}

	private static void HandleIfStatement(SyntaxNodeAnalysisContext context)
	{
		var ifStatement = (IfStatementSyntax)context.Node;
		if (ifStatement.Parent.IsKind(SyntaxKind.ElseClause))
		{
			// this will be analyzed as a clause of the outer if statement
			return;
		}

		List<StatementSyntax> clauses = new List<StatementSyntax>();
		for (IfStatementSyntax current = ifStatement; current != null; current = current.Else?.Statement as IfStatementSyntax)
		{
			clauses.Add(current.Statement);
			if (current.Else != null && !(current.Else.Statement is IfStatementSyntax))
			{
				clauses.Add(current.Else.Statement);
			}
		}

		foreach (StatementSyntax clause in clauses)
		{
			CheckChildStatement(context, clause);
		}
	}

	private static void HandleUsingStatement(SyntaxNodeAnalysisContext context)
	{
		var usingStatement = (UsingStatementSyntax)context.Node;
		if (usingStatement.Statement.IsKind(SyntaxKind.UsingStatement) /* allow consecutive using */)
		{
			return;
		}

		CheckChildStatement(context, usingStatement.Statement);
	}

	private static void CheckChildStatement(SyntaxNodeAnalysisContext context, StatementSyntax childStatement)
	{
		if (childStatement is BlockSyntax)
		{
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(Rule, childStatement.GetLocation()));
	}
}

/// <summary>
/// Implements a code fix for <see cref="BEY0001BracesMustNotBeOmittedAnalyzer"/>.
/// </summary>
/// <remarks>
/// <para>To fix a violation of this rule, the violating statement will be converted to a block statement.</para>
/// </remarks>
[ExportCodeFixProvider(LanguageNames.CSharp)]
public class BEY0001BracesMustNotBeOmittedCodeFix : CodeFixProvider
{
	public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BEY0001BracesMustNotBeOmittedAnalyzer.Rule.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

		foreach (Diagnostic diagnostic in context.Diagnostics)
		{
			if (!(syntaxRoot.FindNode(diagnostic.Location.SourceSpan, false, true) is StatementSyntax node) || node.IsMissing)
			{
				continue;
			}

			// If the parent of the statement contains a conditional directive, stuff will be really hard to fix correctly, so don't offer a code fix.
			if (ContainsConditionalDirectiveTrivia(node.Parent))
			{
				continue;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					Strings.BEY0001BracesMustNotBeOmittedCodeFixTitle,
					cancellationToken => GetTransformedDocumentAsync(context.Document, syntaxRoot, node, cancellationToken),
					nameof(BEY0001BracesMustNotBeOmittedCodeFix)),
				diagnostic);
		}
	}

	private static Task<Document> GetTransformedDocumentAsync(Document document, SyntaxNode root, StatementSyntax node, CancellationToken cancellationToken)
	{
		// Currently unused
		_ = cancellationToken;

		var newSyntaxRoot = root.ReplaceNode(node, SyntaxFactory.Block(node));
		return Task.FromResult(document.WithSyntaxRoot(newSyntaxRoot));
	}

	private static bool ContainsConditionalDirectiveTrivia(SyntaxNode node)
	{
		for (var currentDirective = node.GetFirstDirective(); currentDirective != null && node.Contains(currentDirective); currentDirective = currentDirective.GetNextDirective())
		{
			switch (currentDirective.Kind())
			{
				case SyntaxKind.IfDirectiveTrivia:
				case SyntaxKind.ElseDirectiveTrivia:
				case SyntaxKind.ElifDirectiveTrivia:
				case SyntaxKind.EndIfDirectiveTrivia:
					return true;
			}
		}

		return false;
	}
}
