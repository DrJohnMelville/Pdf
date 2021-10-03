using System.Runtime.CompilerServices;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures
{
    public class S7_9_2StringDataTypes
    {
        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("akhdwfgjkdvgk1")]
        public void RoundTripAscii(string text)
        {
            Assert.Equal(text, PdfString.CreateAscii(text).AsAsciiString());
        }
        [Theory]
        [InlineData("","")]
        [InlineData("a","\xfE\xfF\0a")]
        [InlineData("akh","\xfE\xfF\0a\0k\0h")]
        [InlineData("a₧","þÿ\0a §")]
        public void RoundTripUtf16(string text, string utfAscii)
        {
            Assert.Equal(utfAscii, PdfString.CreateUtf16(text).AsAsciiString());
            var roundTripped = PdfString.CreateUtf16(text).AsUtf16();
            Assert.Equal(text, roundTripped);
        }
    }
}