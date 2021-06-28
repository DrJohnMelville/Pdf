using ArchitectureAnalyzer.Parsers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Parsers
{
    public class RuleLexerTest
    {
        [Theory]
        [InlineData("A.C.* => C.Type+SubType", "A.C.*","=>", "C.Type+SubType")]
        [InlineData("A.C !=> C.Type+SubType", "A.C","!=>", "C.Type+SubType")]
        [InlineData("A.C ^=> C.Type+SubType", "A.C","^=>", "C.Type+SubType")]
        [InlineData("A.C <=> C.Type+SubType", "A.C","<=>", "C.Type+SubType")]
        public void LexRules(string input, string left, string op, string right)
        {
            var sut = new RuleLexer(input);
            VerifyClause(sut, left, op, right);
        }

        private void VerifyClause(RuleLexer sut, string left, string opCode, string RightParam)
        {
            Assert.Equal(opCode, sut.OpCode());
            Assert.Equal(left, sut.LeftParam());
            Assert.Equal(RightParam, sut.RightParam());
        }

        [Fact]
        public void ParseTwoImplications()
        {
            var sut = new RuleLexer("A.C => C.Type+SubType\r\nX=>Y");
            VerifyClause(sut, "A.C", "=>", "C.Type+SubType");
            Assert.True(sut.Next());
            VerifyClause(sut,"X","=>","Y");
            Assert.False(sut.Next());
        }
    }
}