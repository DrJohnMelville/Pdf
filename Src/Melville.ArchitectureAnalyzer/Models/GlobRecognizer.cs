using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Models
{
    public class GlobRecognizer
    {
        private readonly Regex regex;
        public IList<Rule> UseRules { get; } = new List<Rule>();
        public IList<Rule> DeclarationRules { get; } = new List<Rule>();
        public GlobRecognizer(string template)
        {
            regex = GlobRegexFactory.CreateGlobRegex(template);
        }

        public bool Matches(string item) => regex.IsMatch(item);

        public void AddUse(Rule rule) => UseRules.Add(rule);
        public void AddDeclaration(Rule rule) => DeclarationRules.Add(rule);
    }
}