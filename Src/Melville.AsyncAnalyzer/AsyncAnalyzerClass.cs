using System;
using System.Collections.Immutable;
using System.Linq;
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
        context.RegisterSymbolAction(VerifyPropertyNames, SymbolKind.Property);
    }

    private void VerifyMethodNames(SymbolAnalysisContext obj)
    {
        if (obj.Symbol is IMethodSymbol ms and not {IsStatic:true, Name:"Main"})
        {
            new SymbolVerifier(obj, ms, ms.ReturnType).Verify();
        }
    }
    private void VerifyPropertyNames(SymbolAnalysisContext obj)
    {
        if (obj.Symbol is IPropertySymbol {IsIndexer: false} ps) 
            new SymbolVerifier(obj, ps, ps.Type).Verify();
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => AsyncDiagnostics.All;
}

internal partial class SymbolVerifier
{
    [FromConstructor] private readonly SymbolAnalysisContext context;
    [FromConstructor] private readonly ISymbol symbol;
    [FromConstructor] private readonly ITypeSymbol symbolType; 

    public void Verify()
    {
        if (UserCannotChooseName()) return;
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

    private bool UserCannotChooseName() =>
        !symbol.CanBeReferencedByName || 
        symbol.IsImplicitlyDeclared ||
        symbol.IsOverride ||
        IsInterfaceOverride();

    private bool IsInterfaceOverride()
    {
        var type = symbol.ContainingType;
        return type.AllInterfaces
            .SelectMany(i => i.GetMembers().Select(j=>
                type.FindImplementationForInterfaceMember(j)))
            .Any(i=>SymbolEqualityComparer.Default.Equals(i, symbol));
    }

    private bool IsAsyncMethod() => IsAsyncReturnTypeName(TypePrefix(symbolType.ToString()));

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