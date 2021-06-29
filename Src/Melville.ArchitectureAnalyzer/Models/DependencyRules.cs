using System.Collections.Generic;
using System.Linq;

namespace ArchitectureAnalyzer.Models
{
    public interface IDependencyRules
    {
        string? ErrorFromReference(string useLocation, string declarationLocation);
    }

    public class DependencyRules : IDependencyRules
    {
        private readonly RecognizerFactory recognizers = new(new());
        private string? defaultRule;

        public DependencyRules()
        {
            SetStrictMode();
        }

        public string? ErrorFromReference(string useLocation, string declarationLocation) =>
            RulesAppliedToUse(useLocation)
                .Intersect(RulesAppliedToDeclaration(declarationLocation))
                .Select(i=>i.ErrorMessage)
                .DefaultIfEmpty(defaultRule)
                .Select(i=>ExpandErrorMessage(i, useLocation, declarationLocation))
                .First();

        private static string? ExpandErrorMessage(string? brokenRule, string use, string decl)
        {
            return brokenRule == null ? null:
                $"\"{use}\" may not reference \"{decl}\" because \"{brokenRule}\"";
        }

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

        public void DefineGroup(string groupName, IEnumerable<string> groupMembers)
        {
            recognizers.DeclareGroup(groupName, groupMembers);
        }

        public void SetStrictMode() => defaultRule = "No dependency rule found";
        public void SetLooseMode() => defaultRule = null;
    }
}