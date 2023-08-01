using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Moq;
using Xunit;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public abstract partial class StreamTestBase
{
    private readonly string source;
    private readonly string dest;
    private readonly PdfDirectValue compression;
    private readonly PdfDirectValue? parameters;

    protected StreamTestBase(
        string source, string dest, PdfDirectValue compression, PdfDirectValue? parameters = null)
    {
        this.source = source;
        this.dest = dest;
        this.compression = compression;
        this.parameters = parameters;
    }


    private ValueDictionaryBuilder StreamBuilder() =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.FilterTName, compression)
            .WithItem(KnownNames.DecodeParmsTName, parameters??PdfDirectValue.CreateNull());
    
    private PdfValueStream StreamWithPlainTextBacking() =>
        StreamBuilder().AsStream(source);

    private PdfValueStream StreamWithEncodedBacking() =>
        StreamBuilder()
            .WithItem(KnownNames.LengthTName, dest.Length)
            .AsStream(dest.AsExtendedAsciiBytes(),
                StreamFormat.DiskRepresentation);

    [Fact]
    public Task EncodingTestAsync() => VerifyEncodingAsync(StreamWithPlainTextBacking());

    [Fact]
    public async Task DecodeTestAsync()
    {
        await VerifyDecodedStreamAsync(await StreamWithEncodedBacking().StreamContentAsync());
    }
    [Fact]
    public async Task DecodeWhenGettingOneCharAtATimeAsync()
    {
        var src = new OneCharAtATimeStream(dest.AsExtendedAsciiBytes());

        var strSourceMock = MockStreamSource(src);

        var PdfValueStream = StreamBuilder().AsStream(strSourceMock.Object);
        var innerStream = await PdfValueStream.StreamContentAsync();
        await VerifyDecodedStreamAsync(innerStream);
    }


    private async Task VerifyEncodingAsync(PdfValueStream stream) => 
        Assert.Equal(SimulateStreamOutput(), await stream.WriteStreamToStringAsync());
        
    private string SimulateStreamOutput() => 
        $"<</Filter{RenderCompression(compression)}{RenderParams(parameters)}/Length {dest.Length}>> stream\r\n{dest}\r\nendstream";

    private string RenderCompression(PdfDirectValue val)
    {
        if (val.TryGet(out PdfValueArray? arr)) return RenderArray(arr);
        return $"/{val}";
    }

    private string RenderArray(PdfValueArray value) => 
        $"[{string.Join(' ',value.RawItems.Select(i => RenderCompression(i.TryGetEmbeddedDirectValue(out var dir) ? dir : default)))}]";

    private static string RenderParams(PdfDirectValue? parameters) =>
        parameters.HasValue && parameters.Value.TryGet(out PdfValueDictionary? dict)
            ? "/DecodeParms<<" + string.Join("", dict.RawItems.Select(i => $"/{i.Key} {i.Value}")) + ">>"
            : "";

    [Fact]
    public async Task VerifyDisposal1Async()
    { 
        var (streamMock, innerStream) = await DisposeStreamRigAsync();
        streamMock.Verify(i => i.DisposeAsync(), Times.Never);
        await innerStream.DisposeAsync();
        streamMock.Verify(i => i.DisposeAsync(), Times.Once);
    }

    private async Task<(Mock<Stream> streamMock, Stream innerStream)> DisposeStreamRigAsync()
    {
        var streamMock = new Mock<Stream>();
        var strSourceMock = MockStreamSource(streamMock.Object);

        var PdfValueStream = StreamBuilder().AsStream(strSourceMock.Object);
        var innerStream = await PdfValueStream.StreamContentAsync();
        return (streamMock, innerStream);
    }

    private static Mock<IStreamDataSource> MockStreamSource(Stream readFrom)
    {
        var strSourceMock = new Mock<IStreamDataSource>();
        strSourceMock.Setup(i => i.OpenRawStreamAsync(It.IsAny<long>())).ReturnsAsync(readFrom);
        strSourceMock.SetupGet(i => i.SourceFormat).Returns(StreamFormat.DiskRepresentation);
        strSourceMock.Setup(i => i.WrapStreamWithDecryptor(It.IsAny<Stream>())).Returns((Stream s) => s);
        return strSourceMock;
    }

    [Fact]
    public async Task VerifyDisposalAsync()
    {
        var (streamMock, innerStream) = await DisposeStreamRigAsync();

        streamMock.Verify(i => i.Close(), Times.Never);
        innerStream.Dispose();
        streamMock.Verify(i => i.Close(), Times.Once);
    }

    private ValueTask<PdfValueStream> StreamWithEncodedBackEndAsync() =>
         ToStreamWithEncodedBackEndAsync(StreamWithPlainTextBacking());

    private async ValueTask<PdfValueStream> ToStreamWithEncodedBackEndAsync(PdfValueStream str) =>
        new ValueDictionaryBuilder(str.RawItems).AsStream(
            await str.StreamContentAsync(StreamFormat.DiskRepresentation, NullSecurityHandler.Instance),
            StreamFormat.DiskRepresentation);


    [Fact]
    public async Task RoundTripStreamAsync()
    {
        await VerifyDecodedStreamAsync(await (await StreamWithEncodedBackEndAsync()).StreamContentAsync());
    }

    private async Task VerifyDecodedStreamAsync(Stream streamToRead)
    {
        var buf = new byte[source.Length+200];
        var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
        Assert.Equal(source, buf[..read].ExtendedAsciiString());
    }
}