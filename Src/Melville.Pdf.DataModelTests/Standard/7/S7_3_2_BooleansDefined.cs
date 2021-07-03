using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public sealed class S7_3_2_BooleansDefined
    {
        [Theory]
        [InlineData("true /", true, 5)]
        [InlineData("false /", false, 6)]
        public void ParseBoolSucceed(string text, bool value, int nextPosition)
        { 
            var seq = text.AsSequenceReader();
            Assert.True(LiteralTokenParser.TryParse(ref seq, out var item));
            Assert.Equal(nextPosition, seq.Position.GetInteger());
            Assert.True(item is PdfBoolean);
            Assert.Equal(value, ((PdfBoolean)item!).Value);
        }

        [Theory]
        [InlineData("t")]
        [InlineData("tr")]
        [InlineData("tru")]
        [InlineData("f")]
        [InlineData("fa")]
        [InlineData("fal")]
        [InlineData("fals")]
        public void ParseBoolIncomplete(string text)
        { 
            var seq = text.AsSequenceReader();
            Assert.False(LiteralTokenParser.TryParse(ref seq, out _));
        }
    }
}