using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.AshiiHexFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public static class StreamTest
    {
        private static async Task EncodeStreamTest(string dest, PdfStream str, string filterName)
        {
            Assert.Equal(
                $"<</Filter {filterName} /Length {dest.Length}>> stream\r\n{dest}\r\nendstream",
                await str.WriteToString());
        }
        private static async Task RoundTripTestAsync(string src, Stream streamToRead)
        {
            var stream = streamToRead;
            var buf = new byte[src.Length];
            await buf.FillBufferAsync(0, buf.Length, stream);
            Assert.Equal(src, buf.ExtendedAsciiString());
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
            await EncodeStreamTest(dest, str, "/"+compression);
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
}