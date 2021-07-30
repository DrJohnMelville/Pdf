using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.AshiiHexFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public static class StreamTest
    {
        private static async Task EncodeStreamTest(string dest, PdfStream str)
        {
            Assert.Equal(
                $"<</Filter /ASCIIHexDecode /Length {dest.Length}>> stream\r\n{dest}\r\nendstream",
                await str.WriteToString());
        }
        private static async Task RoundTripTestAsync(string src, Stream streamToRead)
        {
            var stream = streamToRead;
            var decoded = await new StreamReader(stream).ReadToEndAsync();
            Assert.Equal(src, decoded);
        }

        private static async Task<Stream> CreateReadingSingleBytes(PdfStream str) =>
            Decompressor.DecodeStream(new OneCharAtAtimeStream(await str.GetRawStream()),
                (await str.GetOrNull(KnownNames.Filter)).AsList(),
                (await str.GetOrNull(KnownNames.Params)).AsList(), int.MaxValue);

        public static async Task Encoding(
            PdfObject compression, PdfObject? parameters, string src, string dest)
        {
            var str = LowLevelDocumentBuilderOperations.NewCompressedStream(
                null!, src, compression, parameters);
            await EncodeStreamTest(dest, str);
            await RoundTripTestAsync(src, await str.GetDecodedStream());
            await RoundTripTestAsync(src, await CreateReadingSingleBytes(str));
            
        }

        public static async Task TestContent(
            string content, string expectedResult, AsciiHexDecoder decoder, PdfObject parameters)
        {
            var source = new MemoryStream(ExtendedAsciiEncoding.AsExtendedAsciiBytes(content));
            var str = decoder.WrapStream(source, parameters);
            var decoded = await new StreamReader(str).ReadToEndAsync();
            Assert.Equal(expectedResult, decoded);
            
        }
    }
    
    public class S7_4_2AsciiHexDecodeFilter
    {
        [Fact]
        public async Task WriteEncodedStream()
        {
            await StreamTest.Encoding(KnownNames.ASCIIHexDecode, null, 
                "Hello World.", "48656C6C6F20576F726C642E");
        }

        [Theory]
        [InlineData("2020", "  ")]
        [InlineData("7>70", "p")]
        [InlineData("2020>", "  ")]
        [InlineData("202>", "  ")]
        [InlineData("20 \r\n\t 20", "  ")]
        public Task SpecialCaseqs(string encoded, string decoded) =>
            StreamTest.TestContent(encoded, decoded, new AsciiHexDecoder(), PdfTokenValues.Null);

    }
}