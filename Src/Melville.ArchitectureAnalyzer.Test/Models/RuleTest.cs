using ArchitectureAnalyzer.Models;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Models;

public class RuleCollectionTest
{
    private readonly DependencyRules sut = new();
    [Fact]
    public void EmptyCollectionAlwaysFalse()
    {
        Assert.Equal("\"a.b\" may not reference \"c.d\" because \"No dependency rule found\"",
            sut.ErrorFromReference("a.b", "c.d"));
    }
    [Fact]
    public void InApplicableRulesDoNotTrigger()
    {
        sut.DeclareRule("c.d", "b.a", "This is the opposite of the tested rule", true);
        Assert.Equal("\"a.b\" may not reference \"c.d\" because \"No dependency rule found\"",
            sut.ErrorFromReference("a.b", "c.d"));
    }

    [Fact]
    public void MayFollowAllowedRule()
    {
        sut.DeclareRule("A.*", "B.*", "djah", true);
        Assert.Null(sut.ErrorFromReference("A.Foo", "B.Bar"));
    }
    [Fact]
    public void ErrorOnForbiddenRule()
    {
        sut.DeclareRule("A.*", "B.*", "Rule Text", false);
        Assert.Equal("\"A.Foo\" may not reference \"B.Bar\" because \"Rule Text\"", sut.ErrorFromReference("A.Foo","B.Bar"));
    }

    [Fact]
    public void OrderDecidesPrecedenceFail()
    {
        sut.DeclareRule("A.*", "B.*", "A.* !=> B.*", false);
        sut.DeclareRule("A.B.*", "B.*", "A.B.* !=> B.*", true);
        Assert.Equal("\"A.B.C\" may not reference \"B.E.F\" because \"A.* !=> B.*\"", sut.ErrorFromReference("A.B.C", "B.E.F"));
    }

    [Fact]
    public void OrderDecidesPrecedenceSucceed()
    {
        sut.DeclareRule("A.B.*", "B.*", "A.B.* !=> B.*", true);
        sut.DeclareRule("A.*", "B.*", "A.* !=> B.*", false);
        Assert.Null(sut.ErrorFromReference("A.B.C", "B.E.F"));
    }
}