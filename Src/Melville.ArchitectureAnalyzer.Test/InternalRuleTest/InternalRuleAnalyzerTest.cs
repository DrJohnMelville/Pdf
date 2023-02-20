using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using ArchitectureAnalyzer;
using ArchitectureAnalyzer.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.InternalRuleTest;

public class InternalRuleAnalyzerTest
{
    [Theory]
    [InlineData("internal class A { public int B;}")]
    [InlineData("""
        /// <summary>
        /// 
        /// </summary>
        public class A { }
        """)]
    public Task NoErrors(string code) => RunTest(code);

    [Theory]
    [InlineData("public class A {}", "A")]
    [InlineData("""
        /// <summary>
        /// 
        /// </summary>
        public class A {
            public A(){}
        }
        """, "A")]
    public Task OneError(string code, string error) => RunTest(code, error);

    [Theory]
    [InlineData("protected int A;")]
    [InlineData("protected internal int A;")]
    [InlineData("public int A;")]
    [InlineData("public int A{get;}")]
    [InlineData("public int A() => 1;")]
    [InlineData("public event System.EventHandler A;")]
    public Task MemberDeclarationFail(string declaration) =>
        OneError(DeclareMemberCode(declaration),"A");
    [Theory]
    [InlineData("""
            /// <summary>
            /// 
            /// </summary>
            public int A;
            """)]
    [InlineData("private int A;")]
    [InlineData("protected private int A;")]
    public Task MemberDeclarationSucceed(string declaration) => NoErrors(DeclareMemberCode(declaration));

    private static string DeclareMemberCode(string declaration) =>
        $$"""
        /// <summary>
        /// 
        /// </summary>
        public class B 
        {
            {{declaration}}
        }
        """;

    [Fact]
    private Task IndexerTest()
    {
        var test = new CSharpAnalyzerTest<DocumentVisibleMembersAnalyzer, XUnitVerifier>
        {
            TestState =
            {
                Sources = { """
                            /// <summary>
                            /// 
                            /// </summary>
                            public class B 
                            {
                                public int this[int x]=> 1;
                            }
                            """
                }
            }
        };
            test.ExpectedDiagnostics.Add(
                new DiagnosticResult(DocumentVisibleMembersDiagnostics.RuleViolated)
                    .WithArguments($"this[] is visible outside the assembly and does not have an Xml comment")
                    .WithLocation(6, 25));

        return test.RunAsync();

    }

    private Task RunTest(string code, params String[] errorLocations)
    {
        var test = new CSharpAnalyzerTest<DocumentVisibleMembersAnalyzer, XUnitVerifier>()
        {
            TestState =
            {
                Sources = { code }
            }
        };
        foreach (var error in errorLocations)
        {
            test.ExpectedDiagnostics.Add(
                new DiagnosticResult(DocumentVisibleMembersDiagnostics.RuleViolated)
                    .WithArguments($"{error} is visible outside the assembly and does not have an Xml comment")
                    .WithLocation(AsLinePosition(code.AsSpan().PositionOf(error))));
        }

        return test.RunAsync();
    }

    private LinePosition AsLinePosition((int Line, int Col) position) => new LinePosition(position.Line-1, position.Col-1);
}