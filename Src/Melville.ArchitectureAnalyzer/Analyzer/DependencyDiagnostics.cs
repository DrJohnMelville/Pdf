using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ArchitectureAnalyzer.Analyzer
{
    public static class DependencyDiagnostics
    {
        public static readonly DiagnosticDescriptor RuleViolated = new(
            "Arch001", "Violated a declared architecture constraint",
            "{0}", "Architecture", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor ParseError = new(
            "Arch002", "Syntax error in architecture definition file",
            "{0}", "Architecture", DiagnosticSeverity.Error, true);

        public static readonly ImmutableArray<DiagnosticDescriptor> All =
            ImmutableArray.Create(
                RuleViolated,
                ParseError
            );
    }
}