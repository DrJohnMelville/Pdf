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

    private bool IsAsyncMethod()
    {
        return symbol.IsAsync || 
               IsTaskOrValueTaskReturnType();
    }

    private static readonly Regex TaskSelector = new Regex(
        @"^System.Threading.Tasks.(?:Value)?Task(?!\w)");

    private bool IsTaskOrValueTaskReturnType()
    {
        return TaskSelector.IsMatch(symbol.ReturnType.ToString());
    }

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