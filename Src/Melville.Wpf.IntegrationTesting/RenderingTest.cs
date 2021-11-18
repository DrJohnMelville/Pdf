using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf;
using Xunit;

namespace Melville.Wpf.IntegrationTesting;

public class RenderingTest
{
    private static IEnumerable<object[]> GeneratorTests() =>
        GeneratorFactory.AllGenerators.Select(i => new object[] { i.Prefix, i });
    
    [WpfTheory]
    [MemberData(nameof(GeneratorTests))]
    public async Task WpfRenderingTest(string shortName, IPdfGenerator generator) => 
        Assert.Equal(shortName, await ComputeWpfHash(generator));

    private Task<string> ComputeWpfHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, HasWpfPage);

    private ValueTask HasWpfPage(PdfPage page, Stream target)
    {
        return RenderToDrawingGroup.RenderToPngStream(page, target);
    }

    [Theory]
    [MemberData(nameof(GeneratorTests))]
    public async Task SkiaRenderingTest(string shortName, IPdfGenerator generator) => 
        Assert.Equal(shortName, await ComputeSkiaHash(generator));

    private static Task<string> ComputeSkiaHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, (page, target) => RenderWithSkia.ToPngStream(page, target, -1, 1024));
    private static async Task<string> ComputeGenericHash(IPdfGenerator generator,
        Func<PdfPage, Stream, ValueTask> renderTo)
    {
        try
        {
            var doc = await ReadDocument(generator);
            var target = new WriteToAdlerStream();
            var firstPage = await (await doc.PagesAsync()).GetPageAsync(0);
            await renderTo(firstPage, target);
            return target.Computer.GetHash().ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private static async Task<PdfDocument> ReadDocument(IPdfGenerator generator)
    {
        MultiBufferStream src = new();
        await generator.WritePdfAsync(src);
        var doc = await PdfDocument.ReadAsync(src.CreateReader());
        return doc;
    }
}

public class WriteToAdlerStream: Stream
{
    public Adler32Computer Computer { get; } = new();
        public override IAsyncResult BeginWrite(
        byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
        throw new NotSupportedException();
    public override void EndWrite(IAsyncResult asyncResult) =>
        throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override int ReadByte() => throw new NotSupportedException();

    public override int Read(Span<byte> buffer) => throw new NotSupportedException();
        
    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer) => Computer.AddData(buffer);

    public override Task WriteAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        Write(buffer.Span);
        return ValueTask.CompletedTask;
    }

    public override void WriteByte(byte value)
    {
        Span<byte> val = stackalloc byte[] { value };
        Write(val);
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public override long Seek(long offset, SeekOrigin origin)=>
        throw new NotSupportedException();

    public override void SetLength(long value)=>
        throw new NotSupportedException();

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanTimeout => false;
    public override bool CanWrite => true;
    public override long Length => 0;
    public override long Position { get; set; }
}