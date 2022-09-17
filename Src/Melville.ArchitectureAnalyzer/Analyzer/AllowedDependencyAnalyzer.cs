using System.Collections.Immutable;
using ArchitectureAnalyzer.Parsers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AllowedDependencyAnalyzer : DiagnosticAnalyzer
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
        context.RegisterSyntaxNodeAction(verifier.CheckTypeAction, SyntaxKind.GenericName);
    }
    
    private static void ReportRuleParseError(CompilationStartAnalysisContext context, string errorMessage)
    {
        context.RegisterSyntaxNodeAction(cac => cac.ReportDiagnostic(
            Diagnostic.Create(DependencyDiagnostics.ParseError, null, errorMessage)), SyntaxKind.CompilationUnit);
    }
}