using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ArchitectureAnalyzer.Models;
using ArchitectureAnalyzer.Parsers;
using Microsoft.CodeAnalysis;

namespace ArchitectureAnalyzer.Analyzer;

public static class AllowedDependencyVerifierFactory
{
    public static AllowedDependencyVerifier Create(IEnumerable<AdditionalText> files) => 
        new(CreateRules(files));

    private static IDependencyRules CreateRules(IEnumerable<AdditionalText> files) => 
        new RuleParser(CombineArchitectureDefinitionFiles(files)).Parse();

    private static string CombineArchitectureDefinitionFiles(IEnumerable<AdditionalText> files) =>
        string.Join(Environment.NewLine,
            files.Where(IsArchitectureDefinitionFile)
                .SelectMany(i => i.GetText()?.Lines));

    public static bool IsArchitectureDefinitionFile(AdditionalText i) => 
        i.Path.EndsWith(".adf", StringComparison.InvariantCultureIgnoreCase);
}