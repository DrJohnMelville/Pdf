﻿using System.Collections.Immutable;
using System.Composition;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using Document = Microsoft.CodeAnalysis.Document;

namespace Melville.AsyncAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncCodeFixProvider))]
[Shared]
public class AsyncCodeFixProvider:CodeFixProvider
{
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is null) return;
        var worker = new AsyncCodeFixer(context, root);
        foreach (var diagnostic in context.Diagnostics)
        {
            switch (diagnostic.Id)
            {
                case "Arch003": 
                    worker.RemoveAsyncSuffix(diagnostic);
                    break;
                case "Arch004": 
                    worker.AddAsyncSuffix(diagnostic);
                    break;
            }
        }
    }

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create("Arch003","Arch004");

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}

public partial struct AsyncCodeFixer
{
    [FromConstructor] private readonly CodeFixContext context;
    [FromConstructor] private readonly SyntaxNode root;

    public void RemoveAsyncSuffix(Diagnostic diagnostic)
    {
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
        if (token.IsMissing) return;
        if (token.ValueText.Length < 6) return;

        var newName = token.ValueText.Substring(0, token.ValueText.Length - 5);

        RegisterFix(diagnostic, token, newName);
    }

    public void AddAsyncSuffix(Diagnostic diagnostic)
    {
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
        if (token.IsMissing) return;

        var newName = token.ValueText + "Async";

        RegisterFix(diagnostic, token, newName);
    }

    private void RegisterFix(Diagnostic diagnostic, SyntaxToken token, string newName)
    {
        var localContext = context;
        var localRoot = root;
        context.RegisterCodeFix(CodeAction.Create(FixName,
            cancellationToken => RenameHelper.RenameSymbolAsync(
                localContext.Document, localRoot, token, newName, cancellationToken),
            nameof(AsyncCodeFixProvider)), diagnostic);
    }

    private static string FixName => "Add Async suffix to name.";
}

public static class RenameHelper
{
    public static async Task<Solution> RenameSymbolAsync(
        Microsoft.CodeAnalysis.Document document, SyntaxNode root, 
        SyntaxToken declarationToken, string newName, 
        CancellationToken cancellationToken)
    {
        var annotatedRoot = 
            root.ReplaceToken(declarationToken, 
            declarationToken.WithAdditionalAnnotations(RenameAnnotation.Create()));
        var annotatedSolution = document.Project.Solution.WithDocumentSyntaxRoot(
            document.Id, annotatedRoot);
        var annotatedDocument = annotatedSolution.GetDocument(document.Id);

        if (annotatedDocument is null) return annotatedSolution;

        annotatedRoot = await annotatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (annotatedRoot is null) return annotatedSolution;
        var annotatedToken = annotatedRoot.FindToken(declarationToken.SpanStart);
        if (annotatedToken.Parent is null) return annotatedSolution;
        var semanticModel = await annotatedDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var symbol = semanticModel?.GetDeclaredSymbol(annotatedToken.Parent, cancellationToken);
        if (symbol == null) return annotatedSolution;

        var newSolution = await Renamer.RenameSymbolAsync(
            annotatedSolution, symbol, 
            new SymbolRenameOptions(false, true, true, false), newName, cancellationToken).ConfigureAwait(false);
        return newSolution;
    }
}