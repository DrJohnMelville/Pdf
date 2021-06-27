using ArchitectureAnalyzer.Models;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Models
{
    public class RuleCollectionTest
    {
        private readonly RuleCollection sut = new();
        [Fact]
        public void EmptyCollectionAlwaysFalse()
        {
            Assert.Equal("No dependency rule found",
                sut.ErrorFromReference("a.b", "c.d"));
        }
        [Fact]
        public void InApplicableRulesDoNotTrigger()
        {
            sut.DeclareRule("c.d", "b.a", "This is the opposite of the tested rule", true);
            Assert.Equal("No dependency rule found",
                sut.ErrorFromReference("a.b", "c.d"));
        }

        [Fact]
        public void MayFollowAllowedRule()
        {
            sut.DeclareRule("A.*", "b.*", "djah", true);
            Assert.Null(sut.ErrorFromReference("A.Foo", "B.Bar"));
        }
        [Fact]
        public void ErrorOnForbiddenRule()
        {
            sut.DeclareRule("A.*", "b.*", "Rule Text", false);
            Assert.Equal("Rule Text", sut.ErrorFromReference("A.Foo","B.Bar"));
        }

        [Fact]
        public void DotCountsDecidePrecedence()
        {
            sut.DeclareRule("A.*", "B.*", "A.* !=> B.*", false);
            sut.DeclareRule("A.B.*", "B.*", "A.B.* !=> B.*", true);
            Assert.Null(sut.ErrorFromReference("A.B.C", "B.E.F"));
        }
    }
}