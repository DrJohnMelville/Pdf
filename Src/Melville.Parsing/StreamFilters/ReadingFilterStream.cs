using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System.Buffers;
using System.IO.Pipelines;

namespace Melville.Parsing.StreamFilters;

/// <summary>
///  Wraps a stream and decodes/encodes with a given filter upon reading.
/// </summary>
public class ReadingFilterStream : DefaultBaseStream
{
    private readonly IStreamFilterDefinition filter;
    private readonly ParsingBuffer source2;
    private bool doneReading = false;

    /// <summary>
    /// Wrap a stream with a given filter.
    /// </summary>
    /// <param name="source">The source stream</param>
    /// <param name="filter">The filter to encode / decode the stream.</param>
    /// <returns>A stream that will read the encoded or decoded data</returns>
    public static Stream Wrap(Stream source, IStreamFilterDefinition filter) =>
        new ReadingFilterStream(source, filter);

    private ReadingFilterStream(Stream sourceStream, IStreamFilterDefinition filter):
        base(true, false, false)
    {
        this.filter = filter;
        source2 = new ParsingBuffer(sourceStream.AsStreamReader()).WithDisposedDependency(sourceStream);
    }

    /// <inheritdoc />
    public override void Close()
    {
        (filter as IDisposable)?.Dispose();
        source2.Dispose();
    }

    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        Close();
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length < 1 || doneReading) return 0;
        var ret = 0;
        do
        {
            ret = HandleResult(buffer.Span);
            if (source2.DoneReadingSource || ret > 0) return ret;
            await source2.ReadBytesAsync().CA();
        } while (ret < 1);
        return ret;
    }
    private int HandleResult(Span<byte> buffer)
    {
        if (doneReading ) return 0;
        var reader = new SpanParsingBuffer(source2.Peek());
        var nextByte = buffer;
        var done = source2.DoneReadingSource ?
            filter.FinalConvert(ref reader, ref nextByte):
            filter.Convert(ref reader, ref nextByte);
        source2.Advance((int)reader.RelativePosition);
        if (done) doneReading = true;
        return buffer.Length - nextByte.Length;
    }

}