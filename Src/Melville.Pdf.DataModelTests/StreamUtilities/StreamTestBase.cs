using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public partial class SingleCharSource : IStreamDataSource
{
    [DelegateTo]
    private readonly IStreamDataSource source;

    public SingleCharSource(IStreamDataSource source)
    {
        this.source = source;
    }

    public async ValueTask<Stream> OpenRawStream(long streamLength)
    {
        return new OneCharAtAtimeStream(await source.OpenRawStream(streamLength));
    }
}

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

    private PdfStream EncodedStreamAsync() =>
        StreamBuilder().AsStream(source);

    private DictionaryBuilder StreamBuilder() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Filter, compression)
            .WithItem(KnownNames.DecodeParms, parameters);

    [Fact]
    public Task EncodingTest() => VerifyEncoding(EncodedStreamAsync());

    [Fact]
    public async Task DecodeTest()
    {
        await VerifyDecodedStream(TrySingleCharStream(await DecodableStream().StreamContentAsync()));
    }

    private PdfStream DecodableStream() =>
        StreamBuilder()
            .WithItem(KnownNames.Length, dest.Length)
            .AsStream(dest.AsExtendedAsciiBytes(),
                StreamFormat.DiskRepresentation);

    public Stream TrySingleCharStream(Stream source) => source;

    private async Task VerifyEncoding(PdfStream stream) => 
        Assert.Equal(SimulateStreamOutput(), await stream.WriteToStringAsync());
        
    private string SimulateStreamOutput() => 
        $"<</Filter{compression}{RenderParams(parameters)}/Length {dest.Length}>> stream\r\n{dest}\r\nendstream";

    private static string RenderParams(PdfObject? parameters) =>
        parameters is not PdfDictionary dict
            ? ""
            : "/DecodeParms<<" + string.Join("", dict.RawItems.Select(i => $"{i.Key} {i.Value}")) + ">>";

    [Fact]
    public async Task VerifyDisposalAsync()
    {
        var strSourceMock = new Mock<IStreamDataSource>();
        var streamMock = new Mock<Stream>();
        streamMock.Setup(i => i.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
        strSourceMock.Setup(i => i.OpenRawStream(It.IsAny<long>())).ReturnsAsync(streamMock.Object);
        strSourceMock.SetupGet(i => i.SourceFormat).Returns(StreamFormat.DiskRepresentation);
        strSourceMock.Setup(i => i.WrapStreamWithDecryptor(It.IsAny<Stream>())).Returns((Stream s) => s);

        var pdfStream = StreamBuilder().AsStream(strSourceMock.Object);
        streamMock.Verify(i=>i.DisposeAsync(), Times.Never);
        var innerStream = await pdfStream.StreamContentAsync();
        await innerStream.DisposeAsync();
        streamMock.Verify(i => i.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task VerifyDisposal()
    {
        var strSourceMock = new Mock<IStreamDataSource>();
        var streamMock = new Mock<Stream>();
        streamMock.Setup(i => i.ReadAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
        strSourceMock.Setup(i => i.OpenRawStream(It.IsAny<long>())).ReturnsAsync(streamMock.Object);
        strSourceMock.SetupGet(i => i.SourceFormat).Returns(StreamFormat.DiskRepresentation);
        strSourceMock.Setup(i => i.WrapStreamWithDecryptor(It.IsAny<Stream>())).Returns((Stream s) => s);

        var pdfStream = StreamBuilder().AsStream(strSourceMock.Object);
        streamMock.Verify(i=>i.Close(), Times.Never);
        var innerStream = await pdfStream.StreamContentAsync();
        innerStream.Dispose();
        streamMock.Verify(i => i.Close(), Times.Once);
    }

    private ValueTask<PdfStream> StreamWithEncodedBackEnd() =>
         ToStreamWithEncodedBackEnd(EncodedStreamAsync());

    private async ValueTask<PdfStream> ToStreamWithEncodedBackEnd(PdfStream str) =>
        new DictionaryBuilder(str.RawItems).AsStream(
            await str.StreamContentAsync(StreamFormat.DiskRepresentation, NullSecurityHandler.Instance),
            StreamFormat.DiskRepresentation);


    [Fact]
    public async Task RoundTripStream()
    {
        await VerifyDecodedStream(await (await StreamWithEncodedBackEnd()).StreamContentAsync());
    }

    private async Task VerifyDecodedStream(Stream streamToRead)
    {
        var buf = new byte[source.Length+200];
        var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
        Assert.Equal(source, buf[..read].ExtendedAsciiString());
    }
}