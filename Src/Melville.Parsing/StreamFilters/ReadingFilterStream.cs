using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.StreamFilters;

/// <summary>
///  Wraps a stream and decodes/encodes with a given filter upon reading.
/// </summary>
public class ReadingFilterStream : DefaultBaseStream
{
    private readonly IStreamFilterDefinition filter;
    private readonly PipeReader source;
    private bool doneReading = false;

    /// <summary>
    /// Wrap a stream with a given filter.
    /// </summary>
    /// <param name="source">The source stream</param>
    /// <param name="filter">The filter to encode / decode the stream.</param>
    /// <returns>A stream that will read the encoded or decoded data</returns>
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

    /// <inheritdoc />
    public override void Close() => source.Complete();

    /// <inheritdoc />
    protected override void Dispose(bool disposing) => source.Complete();

    /// <inheritdoc />
    public override ValueTask DisposeAsync() => source.CompleteAsync();


    /// <inheritdoc />
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
        var (finalPos, bytesWritten, done) = filter.Convert(ref reader, buffer);
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
        (finalPos, extrBytes, done) = filter.FinalConvert(ref r2, remaining);
        bytesWritten += extrBytes;
        return (finalPos, bytesWritten, done);
    }
}