using System;
using System.Collections.Immutable;
using ArchitectureAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllowedDependencyAnalyzer: DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(CheckTypeAction, SyntaxKind.VariableDeclaration);
        }

        private void CheckTypeAction(SyntaxNodeAnalysisContext obj)
        {
            obj.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.RuleViolated,
                obj.Node.GetLocation(), "Diagnostic Text"));
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
          DependencyDiagnostics.All;

    }
}