using System.IO;
using System.Linq;
using ArchitectureAnalyzer.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchitectureAnalyzer.Analyzer
{
    public class AllowedDependencyVerifier
    {
        private readonly IDependencyRules rules;

        public AllowedDependencyVerifier(IDependencyRules rules)
        {
            this.rules = rules;
        }

        public void CheckTypeAction(SyntaxNodeAnalysisContext obj) => 
            new SyntaxNodeAnalysisContextParser(obj, rules).CheckReference();
    }
}