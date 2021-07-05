using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public sealed class S7_3_2_BooleansDefined
    {
        [Theory]
        [InlineData("true /", true)]
        [InlineData("false /", false)]
        public void ParseBoolSucceed(string text, bool value)
        { 
            AAssert.True(text.ParseAs(out PdfBoolean? item));
            Assert.Equal(value, item.Value);
            Assert.True(ReferenceEquals(value?PdfBoolean.True:PdfBoolean.False,item));
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
            Assert.False(text.ParseAs());
        }
    }
}