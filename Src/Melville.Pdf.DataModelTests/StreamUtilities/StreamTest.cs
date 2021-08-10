using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Melville.Hacks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters;
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
            await Decompressor.DecodeStream(new OneCharAtAtimeStream(await str.GetRawStream()),
                (await str.GetOrNull(KnownNames.Filter)).AsList(),
                (await str.GetOrNull(KnownNames.Params)).AsList(), int.MaxValue);

        public static async Task Encoding(
            PdfObject compression, PdfObject? parameters, string src, string dest)
        {
            var str = LowLevelDocumentBuilderOperations.NewCompressedStream(
                null!, src, compression, parameters);
            await EncodeStreamTest(dest, str, compression.ToString()!);
            await VerifyStreamContentAsync(src, await str.GetDecodedStream());
            await VerifyStreamContentAsync(src, new OneCharAtAtimeStream(await CreateReadingSingleBytes(str)));
            await VerifyDisposal(str);
        }

        public static async Task TestContent(
            string encoded, string decoded, IDecoder decoder, PdfObject parameters) =>
            await VerifyStreamContentAsync(decoded,
                await decoder.WrapStreamAsync(StringAsAsciiStream(encoded), parameters));

        private static MemoryStream StringAsAsciiStream(string content) => 
            new(ExtendedAsciiEncoding.AsExtendedAsciiBytes(content));
        
        private static async ValueTask VerifyDisposal(PdfStream str)
        {
            var source = new StreamDisposeSource(await str.GetRawStream());
            var str2 = new PdfStream(str.RawItems, source);
            var wrappedStream = await str2.GetDecodedStream();
            Assert.False(source.IsDisposed);
            await wrappedStream.DisposeAsync();
            Assert.True(source.IsDisposed);
        }

    }

    public partial class StreamDisposeSource : Stream, IStreamDataSource
    {
        public bool IsDisposed { get; private set; }
        [DelegateTo()]
        private readonly Stream source;

        public StreamDisposeSource(Stream source)
        {
            this.source = source;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

        public override void Close() => Dispose(true);

        public ValueTask<Stream> OpenRawStream(long streamLength) => new(this);
    }

}