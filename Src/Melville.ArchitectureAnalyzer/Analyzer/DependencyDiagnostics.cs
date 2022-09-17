using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ArchitectureAnalyzer.Analyzer;

public static class DependencyDiagnostics
{
    public static readonly DiagnosticDescriptor RuleViolated = new DiagnosticDescriptor(
        "Arch001", "Violated a declared architecture constraint",
        "{0}", "Architecture", DiagnosticSeverity.Error, true, 
        "This code element violates architectural constraint defined in the Architecture.Adf file.",
        "https://github.com/DrJohnMelville/Pdf/blob/main/Docs/Architecture/Achitecture.md",
        WellKnownDiagnosticTags.Build);

    public static readonly DiagnosticDescriptor ParseError = new DiagnosticDescriptor(
        "Arch002", "Syntax error in architecture definition file",
        "{0}", "Architecture", DiagnosticSeverity.Error, true,
        "The syntac of the Architecture.Adf file is invalid.",
        "https://github.com/DrJohnMelville/Pdf/blob/main/Docs/Architecture/Achitecture.md",
        WellKnownDiagnosticTags.Build);


    public static readonly ImmutableArray<DiagnosticDescriptor> All =
        ImmutableArray.Create(
            RuleViolated,
            ParseError
        );
}