using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using ArchitectureAnalyzer.Models;
using ArchitectureAnalyzer.Parsers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace ArchitectureAnalyzer.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AllowedDependencyAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            DependencyDiagnostics.All;

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            context.RegisterCompilationStartAction(RegisterDependencyAnalyzer);
        }

        private void RegisterDependencyAnalyzer(CompilationStartAnalysisContext context)
        {
            try
            {
                RegisterNewVerifier(context);
            }
            catch (DslException e)
            {
                ReportRuleParseError(context, e.Message);
            }
        }

        private void RegisterNewVerifier(CompilationStartAnalysisContext context)
        {
            var verifier = AllowedDependencyVerifierFactory.Create(context.Options.AdditionalFiles);
            context.RegisterSyntaxNodeAction(verifier.CheckTypeAction, SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(verifier.CheckTypeAction,  SyntaxKind.GenericName);
        }
        
         #pragma warning disable RS1013
        //The suppressed warning tells us that if all we do in the start action is register an
        // end action we could just register a compiler action instead.  But in the good path we do
        // register actions, this warning is spurious because it does not recognize it is on the error
        // path so the intended registrations did not happen.
        private static void ReportRuleParseError(CompilationStartAnalysisContext context, string errorMessage)
        {
            context.RegisterCompilationEndAction(cac => cac.ReportDiagnostic(
                Diagnostic.Create(DependencyDiagnostics.ParseError, null, errorMessage)));
        }
        #pragma warning restore RS1013
    }
}