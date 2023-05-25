using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Melville.INPC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Melville.AsyncAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncAnalyzerClass: DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(VerifyMethodNames, SymbolKind.Method);
    }

    private void VerifyMethodNames(SymbolAnalysisContext obj)
    {
        if (obj.Symbol is IMethodSymbol ms) new SymbolVerifier(obj, ms).Verify();
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AsyncDiagnostics.All;
}

internal readonly partial struct SymbolVerifier
{
    [FromConstructor] private readonly SymbolAnalysisContext context;
    [FromConstructor] private readonly IMethodSymbol symbol;

    public void Verify()
    {
        switch (IsAsyncMethod(), HasAsyncName())
        {
            case (false, true):
                ReportWarning(AsyncDiagnostics.UnneededAsync);
                break;
            case (true, false):
                ReportWarning(AsyncDiagnostics.AsyncNeeded);
                break;
        }
    }

    private bool IsAsyncMethod() => IsAsyncReturnTypeName(TypePrefix(symbol.ReturnType.ToString()))
    ;

    private bool IsAsyncReturnTypeName(ReadOnlySpan<char> returnTypeName) =>
        returnTypeName is
            "System.Collections.Generic.IAsyncEnumerable" or
            "System.Threading.Tasks.ValueTask" or
            "System.Threading.Tasks.Task";

    private static ReadOnlySpan<char> TypePrefix(string typeName)
    {
        var wakkaIndex = typeName.IndexOf('<');
        var keyLen = wakkaIndex < 0 ? typeName.Length : wakkaIndex;
        var readOnlySpan = typeName.AsSpan(0, keyLen);
        return readOnlySpan;
    }

    // private bool IsAsyncMethod() => TaskSelector.IsMatch(symbol.ReturnType.ToString());
    //
    // private static readonly Regex TaskSelector = new(
    //     @"^(?:(?:System.Threading.Tasks.(?:Value)?Task)(?:System.Collections.Generic.IAsyncEnumerable))(?!\w)");

    private bool HasAsyncName() => symbol.Name.EndsWith("Async");

    private void ReportWarning(DiagnosticDescriptor descriptor)
    {
        foreach (var location in symbol.Locations)
        {
            ReportWarningAt(descriptor, location);
        }
    }

    private void ReportWarningAt(DiagnosticDescriptor diagnosticDescriptor, Location location)
    {
        context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, location,
            symbol.Name));
    }
}