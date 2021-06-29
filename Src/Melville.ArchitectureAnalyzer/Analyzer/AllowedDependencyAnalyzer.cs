using System;
using System.Collections.Immutable;
using ArchitectureAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllowedDependencyAnalyzer: DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
          DependencyDiagnostics.All;

    }
}