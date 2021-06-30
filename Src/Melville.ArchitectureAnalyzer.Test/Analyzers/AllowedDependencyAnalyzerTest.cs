using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchitectureAnalyzer.Analyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Analyzers
{
    public class AllowedDependencyAnalyzerTest
    {
        private static Task RunSimpleTest(string contentOfRelying, string contentOfReliedUpon,
            string? commonContent = null)
        {
            var (source, diagnostics) = 
                ParseSource(ConstructFileText(contentOfRelying, contentOfReliedUpon, commonContent??""));
            var test = new CSharpAnalyzerTest<AllowedDependencyAnalyzer, XUnitVerifier>()
            {
                TestState =
                {
                    Sources = {source},
                    AdditionalFiles =
                    {
                        ("Architecture.adf", "NS.Relying* => NS.ReliedUpon*\r\nNS.R*<=>NS.Common")
                    },
                }
            };
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
            return test.RunAsync();
        }

        private static readonly Regex DiagnosticLocator = new(@"\[\|(.+?)\|\]");
        private static (string Source, List<DiagnosticResult>) ParseSource(string constructFileText)
        {
            var matches = DiagnosticLocator.Matches(constructFileText);
            var diags = matches.Select(MatchToDiagnosticResult).ToList();
            
            return (DiagnosticLocator.Replace(constructFileText, "$1"), diags);
        }

        private static DiagnosticResult MatchToDiagnosticResult(Match match, int i) =>
            new DiagnosticResult(DependencyDiagnostics.RuleViolated).WithLocation(1,
                (match.Index - (4*i))+1).WithArguments("\"NS.ReliedUpon\" may not reference \"NS.Relying\" because \"NS.Relying* => NS.ReliedUpon*\"");

        private static string ConstructFileText(string contentOfRelying, string contentOfReliedUpon, string commonContent)
        {
            return $"namespace NS {{ class Common {{ {commonContent} }} class Relying {{ {contentOfRelying} }} class ReliedUpon{{ {contentOfReliedUpon} }} }}";
        }

        [Fact]
        public Task NoReferenceNoDiagnostic()
        {
            return RunSimpleTest("","");
        }

        [Fact]
        public Task MayRelyOnDeclaredDependency()
        {
            return RunSimpleTest("ReliedUpon item;", "");
        }

        [Fact] public Task CannotDeclarePronibitedField() => RunSimpleTest("", "[|Relying|] item;");
        [Fact] public Task CannotDeclarePronibitedSpecialization() => 
            RunSimpleTest("", "interface I<T> {} I<[|Relying|]> item;");
        [Fact] public Task CannotDeclareProhibitedProperty() =>
            RunSimpleTest("", "[|Relying|] Item {get;}");
        [Fact] public Task CannotDeclareProhibitedLocal() => 
            RunSimpleTest("", "void M() {[|Relying|] item;}");
        [Fact] public Task CannotDeclareProhibitedParameter() => 
            RunSimpleTest("", "void M([|Relying|] item){}");
        [Fact] public Task CannotCallMethodWithProhibitedReturn() => 
            RunSimpleTest("", "void M(){Common.[|Foo()|];}", "public static Relying Foo(){ return null;}");
        
        
        #warning need to test parsing errors in the ADF file
    }
}