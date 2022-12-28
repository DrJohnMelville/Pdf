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
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
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

    [Fact]
    public async Task DecodeTest()
    {
        await VerifyDecodedStream(TrySingleCharStream(await DecodableStream().StreamContentAsync()));
    }

    private PdfStream DecodableStream()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Length, dest.Length)
            .WithItem(KnownNames.Filter, compression)
            .WithItem(KnownNames.DecodeParms, parameters ?? PdfTokenValues.Null)
            .AsStream(dest.AsExtendedAsciiBytes(),
                StreamFormat.DiskRepresentation);
    }

    public Stream TrySingleCharStream(Stream source) => source;
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
        var wrappedStream = await new DictionaryBuilder(str.RawItems).AsStream(dispSource).StreamContentAsync();
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