using ArchitectureAnalyzer.Models;
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

        public void CheckTypeAction(SyntaxNodeAnalysisContext context)
        {
            if (context.EnclosingTypeName() is { } location)
            {
                new SingleUsageDependencyVerifier(context, 
                    i=>rules.ErrorFromReference(location,i)).CheckReference();
            }
        }
    }
}