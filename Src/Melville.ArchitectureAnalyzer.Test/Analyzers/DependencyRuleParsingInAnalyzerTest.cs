using System.Threading.Tasks;
using ArchitectureAnalyzer.Analyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Analyzers;

public class DependencyRuleParsingInAnalyzerTest
{
    private static Task RunSimpleTest(string archRules, DiagnosticResult? diagnostic = null)
    {
        var test = new CSharpAnalyzerTest<AllowedDependencyAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = {"public class Foo {}"},
                AdditionalFiles =
                {
                    ("Architecture.adf", archRules)
                },
            }
        };
        if (diagnostic.HasValue) {test.TestState.ExpectedDiagnostics.Add(diagnostic.Value);}
        return test.RunAsync();
    }

    [Fact]
    public Task EmptyRuleSetParses()
    {
        return RunSimpleTest("");
    }
    [Fact]
    public Task ReportParseError()
    {
        return RunSimpleTest("LGJB", new DiagnosticResult(DependencyDiagnostics.ParseError)
            .WithMessage("\"LGJB\" is not a rule."));
    }
}