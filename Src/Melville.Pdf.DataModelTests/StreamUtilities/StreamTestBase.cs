using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using Moq;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public abstract partial class StreamTestBase
{
    
    private readonly string source;
    private readonly string dest;
    private readonly PdfDirectObject compression;
    private readonly PdfDirectObject? parameters;

    protected StreamTestBase(
        string source, string dest, PdfDirectObject compression, PdfDirectObject? parameters = null)
    {
        this.source = source;
        this.dest = dest;
        this.compression = compression;
        this.parameters = parameters;
    }


    private DictionaryBuilder StreamBuilder() =>
        new DictionaryBuilder()
            .WithItem(KnownNames.Filter, compression)
            .WithItem(KnownNames.DecodeParms, parameters??PdfDirectObject.CreateNull());
    
    private PdfStream StreamWithPlainTextBacking() =>
        StreamBuilder().AsStream(source);

    private PdfStream StreamWithEncodedBacking() =>
        StreamBuilder()
            .WithItem(KnownNames.Length, dest.Length)
            .AsStream(dest.AsExtendedAsciiBytes(),
                StreamFormat.DiskRepresentation);

    [Test]
    public Task EncodingTestAsync() => VerifyEncodingAsync(StreamWithPlainTextBacking());

    [Test]
    public async Task DecodeTestAsync()
    {
        await VerifyDecodedStreamAsync(await StreamWithEncodedBacking().ReaderForContentAsync());
    }
    [Test]
    public async Task DecodeWhenGettingOneCharAtATimeAsync()
    {
        var src = new OneCharAtATimeStream(dest.AsExtendedAsciiBytes());

        var strSourceMock = MockStreamSource(src);

        var PdfValueStream = StreamBuilder().AsStream(strSourceMock.Object);
        var innerStream = await PdfValueStream.ReaderForContentAsync();
        await VerifyDecodedStreamAsync(innerStream);
    }


    private async Task VerifyEncodingAsync(PdfStream stream)
    {
        string encodedString = await stream.WriteStreamToStringAsync();
        Assert.Equal(SimulateStreamOutput(), encodedString);
    }

    private string SimulateStreamOutput() => 
        $"<</Filter{RenderCompression(compression)}{RenderParams(parameters)}/Length {dest.Length}>> stream\r\n{dest}\r\nendstream";

    private string RenderCompression(PdfDirectObject val)
    {
        if (val.TryGet(out PdfArray? arr)) return RenderArray(arr);
        return $"/{val}";
    }

    private string RenderArray(PdfArray value) => 
        $"[{string.Join(' ',value.RawItems.Select(i => RenderCompression(i.TryGetEmbeddedDirectValue(out var dir) ? dir : default)))}]";

    private static string RenderParams(PdfDirectObject? parameters) =>
        parameters.HasValue && parameters.Value.TryGet(out PdfDictionary? dict)
            ? "/DecodeParms<<" + string.Join("", dict.RawItems.Select(i => $"/{i.Key} {i.Value}")) + ">>"
            : "";

    [Test]
    public async Task VerifyDisposal1Async()
    { 
        var (streamMock, innerStream) = await DisposeStreamRigAsync();
        streamMock.Verify(i => i.Close(), Times.Never);
        innerStream.Dispose();
        streamMock.Verify(i => i.Close(), Times.Once);
    }

    private async Task<(Mock<Stream> streamMock, IPdfStreamReader innerStream)> DisposeStreamRigAsync()
    {
        var streamMock = new Mock<Stream>();
        streamMock.SetupGet(i => i.CanRead).Returns(true);
       var strSourceMock = MockStreamSource(streamMock.Object);

        var PdfValueStream = StreamBuilder().AsStream(strSourceMock.Object);
        var innerStream = await PdfValueStream.ReaderForContentAsync();
        return (streamMock, innerStream);
    }

    private static Mock<IStreamDataSource> MockStreamSource(Stream readFrom)
    {
        var strSourceMock = new Mock<IStreamDataSource>();
        var readerMock = new Mock<IPdfStreamReader>();
        readerMock.Setup(i => i.ReadAsync(It.IsAny<Memory<Byte>>(), It.IsAny<CancellationToken>())).Returns(
            (Memory<byte> m, CancellationToken ct) => readFrom.ReadAsync(m, ct));
        readerMock.Setup(i => i.Dispose()).Callback(() => readFrom.Dispose());
        strSourceMock.Setup(i => i.OpenRawReader(It.IsAny<long>())).Returns(readerMock.Object);
        strSourceMock.SetupGet(i => i.SourceFormat).Returns(StreamFormat.DiskRepresentation);
        strSourceMock.Setup(
            i => i.WrapStreamWithDecryptor(It.IsAny<IPdfStreamReader>())).Returns((IPdfStreamReader s) => s);

        return strSourceMock;
    }

    [Test]
    public async Task VerifyDisposalAsync()
    {
        var (streamMock, innerStream) = await DisposeStreamRigAsync();

        streamMock.Verify(i => i.Close(), Times.Never);
        innerStream.Dispose();
        streamMock.Verify(i => i.Close(), Times.Once);
    }

    private ValueTask<PdfStream> StreamWithEncodedBackEndAsync() =>
         ToStreamWithEncodedBackEndAsync(StreamWithPlainTextBacking());

    private async ValueTask<PdfStream> ToStreamWithEncodedBackEndAsync(PdfStream str) =>
        new DictionaryBuilder(str.RawItems).AsStream(
            (await str.StreamEncryptedContentAsync(
                StreamFormat.DiskRepresentation, NullSecurityHandler.Instance, null))
            .AsStreamInefficient(),
            StreamFormat.DiskRepresentation);


    [Test]
    public async Task RoundTripStreamAsync()
    {
        PdfStream pdfStream = await StreamWithEncodedBackEndAsync();
        await VerifyDecodedStreamAsync(await pdfStream.ReaderForContentAsync());
    }

    private async Task VerifyDecodedStreamAsync(IPdfStreamReader streamToRead)
    {
        Assert.Equal(source, await streamToRead.ReadAsStringAsync());
        streamToRead.Dispose();
    }
}