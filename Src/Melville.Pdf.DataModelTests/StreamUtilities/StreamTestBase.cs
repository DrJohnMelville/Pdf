using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities
{
    public abstract class StreamTestBase
    {
        private readonly string source;
        private readonly string dest;
        private readonly PdfObject compression;
        private readonly PdfObject? parameters;

        protected StreamTestBase(
            string source, string dest, PdfObject compression, PdfObject? parameters = null)
        {
            this.source = source;
            this.dest = dest;
            this.compression = compression;
            this.parameters = parameters;
        }

        private ValueTask<PdfStream> EncodedStreamAsync() =>
            LowLevelDocumentBuilderOperations.NewCompressedStream(null,
                source, compression, parameters);

        [Fact]
        public async Task EncodeUsingReading() => await VerifyEncoding(await EncodedStreamAsync());

        private async Task VerifyEncoding(PdfStream stream) => 
            Assert.Equal(SimulateStreamOutput(), await stream.WriteToStringAsync());


        private string SimulateStreamOutput() => 
            $"<</Filter{compression}{RenderParams(parameters)}/Length {dest.Length}>> stream\r\n{dest}\r\nendstream";

        private static string RenderParams(PdfObject? parameters)
        {
            if (parameters is not PdfDictionary dict) return "";
            return "/DecodeParms<<"+string.Join("", dict.RawItems.Select(i => $"{i.Key} {i.Value}"))+">>";
        }

        [Fact]
        public async Task EncodeUsingWriting()
        {
            var stream = await new LowLevelDocumentBuilder(0).NewCompressedStream(
                i => i.WriteAsync(source.AsExtendedAsciiBytes().AsMemory()), 
                compression, parameters);
            await VerifyEncoding(stream);

        }

        [Fact]
        public async Task VerifyDisposal()
        {
            var str = await EncodedStreamAsync();
            var dispSource = new StreamDisposeSource(await str.GetEncodedStreamAsync());
            var wrappedStream = await new PdfStream(dispSource, str.RawItems).GetDecodedStreamAsync();
            Assert.False(dispSource.IsDisposed);
            await wrappedStream.DisposeAsync();
            Assert.True(dispSource.IsDisposed);
        }

        [Fact]
        public async Task RoundTripStream()
        {
            await VerifyDecodedStream(await (await EncodedStreamAsync()).GetDecodedStreamAsync());
        }

        [Fact]
        public async Task ReadSingleCharAtATime()
        {
            var inner = await EncodedStreamAsync();
            var src = new PassthroughStreamSource(
                new OneCharAtAtimeStream(await inner.GetEncodedStreamAsync()));
            var proxy = new PdfStream(src, inner.RawItems);
            await VerifyDecodedStream(new OneCharAtAtimeStream(await proxy.GetDecodedStreamAsync()));
        }
        
        private async Task VerifyDecodedStream(Stream streamToRead)
        {
            var buf = new byte[source.Length+200];
            var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
            Assert.Equal(source, buf[..read].ExtendedAsciiString());
        }
    }
}