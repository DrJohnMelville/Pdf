using ArchitectureAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
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
}