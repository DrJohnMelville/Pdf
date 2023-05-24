using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

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
    [InlineData("a","\xEF\xBB\x00BFa")]
    [InlineData("akh","\xEF\xBB\x00BFakh")]
    [InlineData("a₧","ï»¿aâ‡§")]
    public void RoundTripUtf8(string text, string utfAscii)
    {
        var encoded = PdfString.CreateUtf8(text);
        Assert.Equal(text, encoded.AsUtf8());
        Assert.Equal(text, encoded.AsTextString());
        var asciiRep = encoded.AsAsciiString();
        Assert.Equal(utfAscii, asciiRep);
    }

    [Fact]
    public void CorrectlyDecodeUtf16LE()
    {
        var encoded = new PdfString(new byte[] { 0xFF, 0xFE, 0x41, 0, 0x42, 0 });
        Assert.Equal("AB", encoded.AsTextString());
        
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

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("akh")]
    [InlineData("a\u2014b")]
    public async Task TextStreamTest(string text)
    {
        var utf8 = new DictionaryBuilder().AsStream(PdfString.CreateUtf8(text).Bytes);
        Assert.Equal(text, await (await utf8.TextStreamReaderAsync()).ReadToEndAsync());
        var utf = new DictionaryBuilder().AsStream(PdfString.CreateUtf16(text).Bytes);
        Assert.Equal(text, await (await utf.TextStreamReaderAsync()).ReadToEndAsync());

        var pdfEnc = new DictionaryBuilder().AsStream(PdfString.CreatePdfEncoding(text).Bytes);
        Assert.Equal(text, await (await pdfEnc.TextStreamReaderAsync()).ReadToEndAsync());
            
    }
}