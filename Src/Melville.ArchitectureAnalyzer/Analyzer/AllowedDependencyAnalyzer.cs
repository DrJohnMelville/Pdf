using System;
using System.Collections.Immutable;
using System.Linq;
using ArchitectureAnalyzer.Models;
using ArchitectureAnalyzer.Parsers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            context.RegisterSyntaxNodeAction(verifier.CheckTypeAction, SyntaxKind.VariableDeclaration);
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

    public readonly struct AllowedDependencyVerifier
    {
        private readonly IDependencyRules rules;

        public AllowedDependencyVerifier(IDependencyRules rules)
        {
            this.rules = rules;
        }

        public void CheckTypeAction(SyntaxNodeAnalysisContext obj)
        {
            var parser = new SyntaxNodeAnalysisContextParser(obj);
            var location = parser.EnclosingTypeName();
            var typeString = parser.ReferencedTypeName();
            if (
                location is not null && typeString is not null &&
                rules.ErrorFromReference(location, typeString) is {} errorMessage)
            {
                obj.ReportDiagnostic(Diagnostic.Create(DependencyDiagnostics.RuleViolated,
                    obj.Node.GetLocation(), errorMessage));
            }
        }
    }

    public readonly struct SyntaxNodeAnalysisContextParser
    {
        private readonly SyntaxNodeAnalysisContext context;

        public SyntaxNodeAnalysisContextParser(SyntaxNodeAnalysisContext context)
        {
            this.context = context;
        }
        public string? ReferencedTypeName()
        {
            return TypeSyntax(context) is not {} typeSyntax? null:
                context.SemanticModel.GetSymbolInfo(typeSyntax).Symbol?.ToString();
        }

        private TypeSyntax? TypeSyntax(SyntaxNodeAnalysisContext context) =>
            context.Node switch
            {
                VariableDeclarationSyntax vds => vds.Type,
                _ => null
            };

        public string? EnclosingTypeName() =>
            EnclosingType(context.Node) is {} decl ? DeclaredTypeName(decl): null;

        private string? DeclaredTypeName(TypeDeclarationSyntax typeDecl) =>
            context.SemanticModel.GetDeclaredSymbol(typeDecl)?.ToString();

        private TypeDeclarationSyntax? EnclosingType(SyntaxNode? syntaxNode) =>
            syntaxNode switch
            {
                null => null,
                TypeDeclarationSyntax tds => tds,
                _ => EnclosingType(syntaxNode.Parent)
            };
    } 
}