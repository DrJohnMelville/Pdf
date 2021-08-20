using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Hacks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities
{
    public static class StreamTest
    {
        private static async Task EncodeStreamTest(string dest, PdfStream str, string filterName)
        {
            Assert.Equal(
                $"<</Filter {filterName} /Length {dest.Length}>> stream\r\n{dest}\r\nendstream",
                await str.WriteToString());
        }
        private static async Task VerifyStreamContentAsync(string src, Stream streamToRead)
        {
            var buf = new byte[src.Length+200];
            var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
            Assert.Equal(src, buf[..read].ExtendedAsciiString());
        }

        private static async Task<Stream> CreateReadingSingleBytes(PdfStream str) =>
            await Decoder.DecodeStream(new OneCharAtAtimeStream(await str.GetEncodedStream()),
                (await str.GetOrNull(KnownNames.Filter)).AsList(),
                (await str.GetOrNull(KnownNames.Params)).AsList(), int.MaxValue);

        public static async Task Encoding(
            PdfObject compression, PdfObject? parameters, string src, string dest)
        {
            var str = await LowLevelDocumentBuilderOperations.NewCompressedStream(
                null!, src, compression, parameters);
            await EncodeUsingWrite(compression, parameters, src, dest);
            await EncodeStreamTest(dest, str, compression.ToString()!);
            await VerifyStreamContentAsync(src, await str.GetDecodedStream());
            await VerifyStreamContentAsync(src, new OneCharAtAtimeStream(await CreateReadingSingleBytes(str)));
            await VerifyDisposal(str);
        }

        private static async Task EncodeUsingWrite(PdfObject compression, PdfObject? parameters,
            string src, string dest)
        {
            var stream = await new LowLevelDocumentBuilder(0).NewCompressedStream(
                i => i.WriteAsync(src.AsExtendedAsciiBytes().AsMemory()), compression, parameters);
            
            var target = new MultiBufferStream();
            await (await stream.GetEncodedStream()).CopyToAsync(target);
            Assert.Equal(dest, target.CreateReader().ReadToArray().ExtendedAsciiString());
        }

        public static async Task TestContent(
            string encoded, string decoded, PdfName decoder, PdfObject parameters) =>
            await VerifyStreamContentAsync(decoded,
                await CodecFactory.CodecFor(decoder).DecodeOnReadStream(StringAsAsciiStream(encoded), parameters));

        private static MemoryStream StringAsAsciiStream(string content) => 
            new(ExtendedAsciiEncoding.AsExtendedAsciiBytes(content));
        
        private static async ValueTask VerifyDisposal(PdfStream str)
        {
            var source = new StreamDisposeSource(await str.GetEncodedStream());
            var str2 = new PdfStream(str.RawItems, source);
            var wrappedStream = await str2.GetDecodedStream();
            Assert.False(source.IsDisposed);
            await wrappedStream.DisposeAsync();
            Assert.True(source.IsDisposed);
        }

    }
}