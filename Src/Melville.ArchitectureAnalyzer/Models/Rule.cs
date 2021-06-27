using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ArchitectureAnalyzer.Models
{
    public readonly struct Rule
    {
        public string Declaration { get; }
        public string? ErrorMessage => Allowed ? null : Declaration;
        public bool Allowed { get; }
        public int Dots => Declaration.OfType<char>().Count(i => i == '.');
        public Rule(string declaration, bool allowed)
        {
            Declaration = declaration;
            Allowed = allowed;
        }
    }

    public class RuleCollection
    {
        private readonly RecognizerFactory recognizers = new(new());
        public string? ErrorFromReference(string useLcation, string declarationLocation) =>
            RulesAppliedToUse(useLcation)
                .Union(RulesAppliedToDeclaration(declarationLocation))
                .OrderByDescending(i=>i.Dots)
                .ThenByDescending(i=>i.Declaration.Length)
                .Select(i=>i.ErrorMessage)
                .DefaultIfEmpty("No dependency rule found")
                .First();

        private IEnumerable<Rule> RulesAppliedToDeclaration(string declarationLocation) => 
            recognizers.MatchingRecognizers(declarationLocation).SelectMany(i=>i.DeclarationRules);

        private IEnumerable<Rule> RulesAppliedToUse(string useLcation) => 
            recognizers.MatchingRecognizers(useLcation).SelectMany(i=>i.UseRules);

        public void DeclareRule(string use, string declaration, string fullDeclaration, bool allowed)
        {
            var useRule = recognizers.Create(use);
            var declarationRule = recognizers.Create(declaration);
            var rule = new Rule(fullDeclaration, allowed);
            useRule.AddUse(rule);
            declarationRule.AddDeclaration(rule);
        }
    }
}