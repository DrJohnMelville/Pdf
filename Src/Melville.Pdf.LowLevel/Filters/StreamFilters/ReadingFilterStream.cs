using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters;

public class ReadingFilterStream : DefaultBaseStream
{
    private readonly IStreamFilterDefinition filter;
    private PipeReader source;
    private bool doneReading = false;

    public static Stream Wrap(Stream source, IStreamFilterDefinition filter)
    {
        var ret = new ReadingFilterStream(source, filter);
        return EmsureMinimumReadSizes(filter, ret);
    }

    private static Stream EmsureMinimumReadSizes(
        IStreamFilterDefinition filter, ReadingFilterStream ret) =>
        filter.MinWriteSize > 1?
            new MinimumReadSizeFilter(ret, filter.MinWriteSize):
            ret;

    private ReadingFilterStream(Stream sourceStream, IStreamFilterDefinition filter):
        base(true, false, false)
    {
        this.filter = filter;
        this.source = PipeReader.Create(sourceStream);
    }
    public override void Close() => source.Complete();
    protected override void Dispose(bool disposing) => source.Complete();
    public override ValueTask DisposeAsync() => source.CompleteAsync();

        
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length < 1 || doneReading ) return 0;
        var ret = 0;
        do
        {
            var result = await source.ReadAsync().CA();
            ret = HandleResult(buffer.Span, result);
            if (result.IsCompleted) return ret;
        } while (ret < 1);
        return ret;
    }

    private int HandleResult(Span<byte> buffer, ReadResult result)
    {
        if (result.IsCanceled || doneReading) return 0;
        var reader = new SequenceReader<byte>(result.Buffer);
        var (finalPos, bytesWritten, done) = filter.Convert(ref reader, ref buffer);
        if (bytesWritten == 0 && result.IsCompleted)
        {
            (finalPos, bytesWritten, done) = HandleFinalDecode(buffer, result, finalPos, bytesWritten);
        }
        source.AdvanceTo(finalPos, result.Buffer.End);
        if (done) doneReading = true;
        Position += bytesWritten;
        return bytesWritten;
    }

    private (SequencePosition finalPos, int bytesWritten, bool done) HandleFinalDecode(Span<byte> buffer, ReadResult result,
        SequencePosition finalPos, int bytesWritten)
    {
        bool done;
        int extrBytes;
        var r2 = new SequenceReader<byte>(result.Buffer.Slice(finalPos));
        var remaining = buffer.Slice(bytesWritten);
        (finalPos, extrBytes, done) = filter.FinalConvert(ref r2, ref remaining);
        bytesWritten += extrBytes;
        return (finalPos, bytesWritten, done);
    }
}