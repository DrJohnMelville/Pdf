using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Models;

public class GlobRecognizer
{
    private readonly bool match = true;
    private readonly Regex regex;
    public IList<Rule> UseRules { get; } = new List<Rule>();
    public IList<Rule> DeclarationRules { get; } = new List<Rule>();
    public GlobRecognizer(string template)
    {
        int i;
        for (i = 0; i < template.Length && template[i] == '!'; i++) match = !match;
        regex = GlobRegexFactory.CreateGlobRegex(template.Substring(i));
    }

    public GlobRecognizer(IEnumerable<string> options)
    {
        regex = GlobRegexFactory.CreateMultiGlobRecognizer(options);
    }

    public bool Matches(string item) => regex.IsMatch(item) == match;

    public void AddUse(Rule rule) => UseRules.Add(rule);
    public void AddDeclaration(Rule rule) => DeclarationRules.Add(rule);
}