using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamDataSources;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
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
        new DictionaryBuilder()
            .WithItem(KnownNames.Filter, compression)
            .WithItem(KnownNames.DecodeParms, parameters)
            .AsStream(source);
        
    [Fact]
    public Task EncodingTest() => VerifyEncoding(EncodedStreamAsync());

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DecodeTest(bool singleChars)
    {
        await VerifyDecodedStream(TrySingleCharStream(await DecodableStream(singleChars).StreamContentAsync(), singleChars));
    }

    private PdfStream DecodableStream(bool singleCharStream)
    {
        return new PdfStream(TrySingleCharSource(new LiteralStreamSource(
                new MultiBufferStream(dest.AsExtendedAsciiBytes()),
                StreamFormat.DiskRepresentation), singleCharStream),
            new Dictionary<PdfName, PdfObject>()
            {
                {KnownNames.Length, new PdfInteger(dest.Length)},
                {KnownNames.Filter, compression},
                {KnownNames.DecodeParms, parameters ?? PdfTokenValues.Null}
            }
        );
    }

    public Stream TrySingleCharStream(Stream source, bool asSingleChars) =>
        asSingleChars ? new OneCharAtAtimeStream(source) : source;
    public IStreamDataSource TrySingleCharSource(IStreamDataSource source, bool asSingleChars) =>
        asSingleChars ? new SingleCharSource(source) : source;

    private async Task VerifyEncoding(PdfStream stream) => 
        Assert.Equal(SimulateStreamOutput(), await stream.WriteToStringAsync());
        
    private string SimulateStreamOutput() => 
        $"<</Filter{compression}{RenderParams(parameters)}/Length {dest.Length}>> stream\r\n{dest}\r\nendstream";

    private static string RenderParams(PdfObject? parameters) =>
        parameters is not PdfDictionary dict
            ? ""
            : "/DecodeParms<<" + string.Join("", dict.RawItems.Select(i => $"{i.Key} {i.Value}")) + ">>";

    [Fact]
    public async Task VerifyDisposal()
    {
        var str = EncodedStreamAsync();
        var dispSource = new StreamDisposeSource(await str.StreamContentAsync(), StreamFormat.PlainText);
        var wrappedStream = await new PdfStream(dispSource, str.RawItems).StreamContentAsync();
        Assert.False(dispSource.IsDisposed);
        await wrappedStream.DisposeAsync();
        Assert.True(dispSource.IsDisposed);
    }

    [Fact]
    public async Task RoundTripStream()
    {
        await VerifyDecodedStream(await EncodedStreamAsync().StreamContentAsync());
    }

    private async Task VerifyDecodedStream(Stream streamToRead)
    {
        var buf = new byte[source.Length+200];
        var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
        Assert.Equal(source, buf[..read].ExtendedAsciiString());
    }
}