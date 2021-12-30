using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchitectureAnalyzer.Analyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Analyzers;

public class AllowedDependencyAnalyzerTest
{
    private static Task RunSimpleTest(string contentOfRelying, string contentOfReliedUpon,
        string? commonContent = null, int howManyErrors = 1) =>
        RunSimpleTest(
            ConstructFileText(contentOfRelying, contentOfReliedUpon, commonContent ?? ""),
            howManyErrors);

    private static Task RunSimpleTest(string sourceText, int howManyErrors = 1, string? errorMsg = null)
    {
        var (source, diagnostics) =
            ParseSource(sourceText, errorMsg??
                                    "\"NS.ReliedUpon\" may not reference \"NS.Relying\" because \"NS.Relying* => NS.ReliedUpon*\"");
        var test = new CSharpAnalyzerTest<AllowedDependencyAnalyzer, XUnitVerifier>()
        {
            TestState =
            {
                Sources = {source},
                AdditionalFiles =
                {
                    ("Architecture.adf", "NS.Relying* => NS.ReliedUpon*\r\nNS.R*<=>NS.Common*\r\nGroup Com\r\n  NS.Common*")
                },
            }
        };
        for (int i = 0; i < howManyErrors; i++)
        {
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
        }
        return test.RunAsync();
    }

    private static readonly Regex DiagnosticLocator = new(@"\[\|(.+?)\|\]");
    private static (string Source, List<DiagnosticResult>) ParseSource(string constructFileText, string errorMsg)
    {
        var matches = DiagnosticLocator.Matches(constructFileText);
        var diags = matches.Select((match, i) => MatchToDiagnosticResult(match, i, errorMsg)).ToList();
            
        return (DiagnosticLocator.Replace(constructFileText, "$1"), diags);
    }

    private static DiagnosticResult MatchToDiagnosticResult(Match match, int i, string errorMsg) =>
        new DiagnosticResult(DependencyDiagnostics.RuleViolated).WithLocation(1,
                (match.Index - (4*i))+1)
            .WithArguments(errorMsg);

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

    [Fact] public Task CannotDeclareProhibitedField() => RunSimpleTest("", "[|Relying|] item;");
    [Fact] public Task CannotDeclareProhibitedArray() => RunSimpleTest("", "[|Relying|][] item;");
    [Fact] public Task CannotDeclareProhibitedSpecialization() => 
        Assert.ThrowsAsync<EqualWithMessageException>(
            ()=> RunSimpleTest("", " Common.[|I<Relying>|] item;", "public interface I<T> {}", 2));
    [Fact] public Task CanDeclareAllowedSpecialization() => 
        RunSimpleTest("", " Common.I<int> item;", "public interface I<T> {}");
    [Fact] public Task CanDeclareAllowedSpecializationWithNullabe() => 
        RunSimpleTest("", " Common.I<int?> item;", "public interface I<T> {}");
    [Fact] public Task CannotDeclareProhibitedProperty() =>
        RunSimpleTest("", "[|Relying|] Item {get;}");
    [Fact] public Task CannotDeclareProhibitedLocal() => 
        RunSimpleTest("", "void M() {[|Relying|] item;}");
    [Fact] public Task CannotDeclareProhibitedParameter() => 
        RunSimpleTest("", "void M([|Relying|] item){}");
    [Fact] public Task CannotCallMethodWithProhibitedReturn() => 
        RunSimpleTest(
            "", "void M(){Common.[|Foo|]();}",
            "public static Relying Foo(){ return null;}");
    [Fact] public Task CannotCallPropertyWithProhibitedReturn() => 
        RunSimpleTest(
            "", "void X(object o){} void M(){X(Common.[|Foo|]);}",
            "public static Relying Foo=> null;");
    [Fact] public Task CanRelyOnArraysOfAcceptableTypes() => 
        RunSimpleTest(
            "", "void X(object o){} void M(){X(Common.Foo);}",
            "public static byte[] Foo=> null;");
    [Fact] public Task CannotRelyOnArraysOfProhibitedTypes() => 
        RunSimpleTest(
            "", "void X(object o){} void M(){X(Common.[|Foo|]);}",
            "public static Relying[] Foo=> null;");
    [Fact] public Task CannotCallMethodWithProhibitedParameter() => 
        RunSimpleTest(
            "", "void M(){Common.[|Foo|](null);}", 
            "public static void Foo(Relying d){ }");
    [Fact] public Task CannotCallMethodWithProhibitedGenericParameter() => 
        RunSimpleTest(
            "", "void M(){Common.[|Foo|](null);}", 
            "public interface I<T> {} public static void Foo(I<Relying> d){ }");
    [Fact] public Task CannotAttachEventReturningProhibitedType() => 
        RunSimpleTest(
            "", "void M(){Common.[|DelEvent|] += i=>{};}", 
            "public delegate void Del(Relying r); public static event Del DelEvent;", 2);
    [Fact] public Task CanDeclareGenerics() => 
        RunSimpleTest(
            "", "T M<T>(T input){return input;}", 
            "");
    [Fact] public Task CanUseTuples() => 
        RunSimpleTest(
            "", "(int, double) M(){return (1,12.3);} void N(){var (x,y) = M();}", 
            "");
    [Fact] public Task CanUsePointer() => 
        RunSimpleTest(
            "", "unsafe byte* M() => (byte*)0; unsafe void N(){var x = M();}", 
            "");
    [Fact] public Task CannotCallStaticOnProhibitedType() => 
        RunSimpleTest(
            "public static void M(){}", "void M(){[|Relying|].M();}");
    [Fact] public Task CannotInheritFromProhibitedType() => 
        RunSimpleTest("namespace NS { public class ReliedUpon: [|Relying|]{} public class Relying{}}");
    [Fact] public Task CannotInheritFromProhibitedGenericType() => 
        RunSimpleTest("namespace NS { public class ReliedUpon: [|Relying<int>|]{} public class Relying<T>{}}", 
            1, "\"NS.ReliedUpon\" may not reference \"NS.Relying<int>\" because \"NS.Relying* => NS.ReliedUpon*\"");
    
}