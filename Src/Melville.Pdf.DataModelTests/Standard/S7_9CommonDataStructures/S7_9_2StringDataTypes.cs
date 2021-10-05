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
            var encoded = PdfString.CreateUtf16(text);
            Assert.Equal(utfAscii, encoded.AsAsciiString());
            Assert.Equal(text, encoded.AsUtf16());
            Assert.Equal(text, encoded.AsTextString());
        }
        [Theory]
        [InlineData("","")]
        [InlineData("a","a")]
        [InlineData("akh","akh")]
        [InlineData("a\u2014b","a—b")]
        public void RoundTripPdfDoc(string text, string utfAscii)
        {
            var encoded = PdfString.CreatePdfEncoding(text);
            Assert.Equal(text, encoded.AsPdfDocEnccodedString());
            Assert.Equal(text, encoded.AsTextString());
            Assert.Equal(utfAscii, encoded.AsAsciiString());
        }
    }
}