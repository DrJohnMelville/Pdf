using System.Collections.Immutable;
using System.Composition;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.INPC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Melville.AsyncAnalyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncCodeFixProvider))]
[Shared]
public class AsyncCodeFixProvider:CodeFixProvider
{
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        (await CreateCodeFixWorker(context))?.FixAll();
    }

    private static async Task<AsyncCodeFixer?> CreateCodeFixWorker(CodeFixContext context) =>
        (await context.Document.GetSyntaxRootAsync(context.CancellationToken)) is { } root
            ? new AsyncCodeFixer(context, root)
            : null;

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create("Arch003","Arch004");

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}

public partial struct AsyncCodeFixer
{
    [FromConstructor] private readonly CodeFixContext context;
    [FromConstructor] private readonly SyntaxNode root;

    public void FixAll()
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            FixOneDiagnostic(diagnostic);
        }
    }

    private void FixOneDiagnostic(Diagnostic diagnostic)
    {
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
        if (token.IsMissing) return;
        AddOrRemoveSuffix(diagnostic, token);
    }

    private void AddOrRemoveSuffix(Diagnostic diagnostic, SyntaxToken token)
    {
        switch (diagnostic.Id)
        {
            case "Arch003":
                RemoveAsyncSuffix(diagnostic, token);
                break;
            case "Arch004":
                AddAsyncSuffix(diagnostic, token);
                break;
        }
    }

    public void RemoveAsyncSuffix(Diagnostic diagnostic, SyntaxToken token)
    {
        if (token.ValueText.Length < 6) return;
        var newName = token.ValueText.Substring(0, token.ValueText.Length - 5);
        RegisterFix(diagnostic, token, newName, "Remove Async suffix from name.");
    }

    public void AddAsyncSuffix(Diagnostic diagnostic, SyntaxToken token)
    {
        var newName = token.ValueText + "Async";
        RegisterFix(diagnostic, token, newName, "Add Async suffix to name.");
    }

    private void RegisterFix(Diagnostic diagnostic, SyntaxToken token, string newName, string fixName)
    {
        var localContext = context;
        var localRoot = root;
        context.RegisterCodeFix(CodeAction.Create(fixName,
            cancellationToken => RenameHelper.RenameSymbolAsync(
                localContext.Document, localRoot, token, newName, cancellationToken),
            nameof(AsyncCodeFixProvider)), diagnostic);
    }
}