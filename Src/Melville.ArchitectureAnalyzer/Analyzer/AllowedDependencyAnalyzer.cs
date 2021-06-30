using System;
using System.Collections.Immutable;
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
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            context.RegisterCompilationStartAction(OnCompilerStart);

        }

        private void OnCompilerStart(CompilationStartAnalysisContext context)
        {
            var verifier = new AllowedDependencyVerifier(CreateRules(context.Options.AdditionalFiles));
            context.RegisterSyntaxNodeAction(verifier.CheckTypeAction, SyntaxKind.IdentifierName);
        }

        private IDependencyRules CreateRules(ImmutableArray<AdditionalText> files)
        {
            var text =
                string.Join(Environment.NewLine,
                    files.Where(IsArchitectureDefinitionFile)
                        .SelectMany(i => i.GetText()?.Lines));
            return new RuleParser(text).Parse();
        }

        private bool IsArchitectureDefinitionFile(AdditionalText i)
        {
            return i.Path.EndsWith(".adf", StringComparison.InvariantCultureIgnoreCase);
        }


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
          DependencyDiagnostics.All;

    }
}