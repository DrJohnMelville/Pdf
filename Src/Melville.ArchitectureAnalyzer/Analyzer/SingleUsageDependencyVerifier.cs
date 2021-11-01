using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer;

public readonly struct SingleUsageDependencyVerifier
{
    private readonly SyntaxNodeAnalysisContext context;
    private readonly Func<string, string?> errorFromTypeUsed;

    public SingleUsageDependencyVerifier(
        SyntaxNodeAnalysisContext context, Func<string, string?> errorFromTypeUsed)
    {
        this.context = context;
        this.errorFromTypeUsed = errorFromTypeUsed;
    }
        
    public void CheckReference()
    {
        if ( context.Node is TypeSyntax ts) CheckTypeUsage(ts);
    }

    private void CheckTypeUsage(TypeSyntax typeSyntax) => 
        CheckTypeUsage(context.SemanticModel.GetSymbolInfo(typeSyntax).Symbol);

    private void CheckTypeUsage(ISymbol? symbol)
    {
        switch (symbol)
        {
            case ITypeParameterSymbol: 
                return; // all type parameters are legal because we are defining a generic
            case IEventSymbol es:
                CheckEventUsage(es);
                break;
            case IPropertySymbol ps:
                CheckTypeUsage(ps.Type);
                break;
            case IMethodSymbol ms:
                CheckMethodCall(ms);
                break;
            case IArrayTypeSymbol ats:
                CheckTypeUsage(ats.ElementType);
                break;
            case INamedTypeSymbol {Name:"ValueTuple"} nts:
                CheckSymbolList(nts.TypeArguments);
                break;
            case ITypeSymbol ts:
                CheckTypeSymbolUsage(ts);
                break;
            // implicitly - null and unrecognized symbol types do nothing
        }
    }

    private void CheckEventUsage(IEventSymbol es)
    {
        CheckTypeUsage(es.Type);
        CheckTypeUsage(es.AddMethod);
    }

    private void CheckMethodCall(IMethodSymbol ms)
    {
        CheckTypeUsage(ms.ReturnType);
        CheckSymbolList(ms.Parameters.Select(i => i.Type));
    }

    private void CheckTypeSymbolUsage(ITypeSymbol? usedType)
    {
            
        if (usedType is not {SpecialType: SpecialType.None}) return;
        if (usedType is INamedTypeSymbol nts && nts.ConstructedFrom.ToString() == "System.Nullable<T>")
        {
            CheckTypeSymbolUsage(nts.TypeArguments[0]);
            return;
        }
        ComputeUsageDiagnostics(usedType);
        CheckDependantTypes(usedType);
    }

    private void CheckDependantTypes(ITypeSymbol? usedType)
    {
        if (usedType is not INamedTypeSymbol nts) return;
        CheckTypeUsage(nts.DelegateInvokeMethod);
        CheckSymbolList(nts.TypeArguments);
    }

    private void CheckSymbolList(IEnumerable<ITypeSymbol> symbols)
    {
        foreach (var parameterSymbol in symbols) CheckTypeUsage(parameterSymbol);
    }

    private void ComputeUsageDiagnostics(ITypeSymbol usedType)
    {
        if (ApplyReferenceRules(usedType) is { } errorMessage)
            context.ReportDiagnostic(CreateDiagnostic(errorMessage));
    }

    private string? ApplyReferenceRules(ITypeSymbol usedType) =>
        usedType.ToString() is {} useStr ? errorFromTypeUsed(useStr):null;

    private Diagnostic CreateDiagnostic(string? errorMessage) =>
        Diagnostic.Create(DependencyDiagnostics.RuleViolated, 
            context.Node.GetLocation(), errorMessage);
}