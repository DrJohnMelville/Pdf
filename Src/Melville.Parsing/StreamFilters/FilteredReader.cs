using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Parsing.StreamFilters;


/// <summary>
/// This implements an IAsyncReader that uses the given StreamFilterDefinition to read and decode the stream.
/// </summary>
/// <param name="inner">Source of the encoded data.</param>
/// <param name="filter">An IStreamFilterDefinition to do the decoding.</param>
public class FilteredReader(IAsyncReader inner, IStreamFilterDefinition filter) : IAsyncReader, IDisposable
{
    private ParsingBuffer input = new ParsingBuffer(inner);
    bool isDone = false;

    ///<inheritdoc/>

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        int bytesRead = 0;
        while (!isDone && !buffer.IsEmpty && (bytesRead += TryReadFromBuffer(buffer)) is 0)
            await input.ReadBytesAsync();
        return bytesRead;
    }

    int TryReadFromBuffer(Memory<byte> target)
    {
        var reader = new SpanParsingBuffer(input.Peek());
        var targetSpan = target.Span;
        isDone = input.DoneReadingSource ?
            filter.FinalConvert(ref reader, ref targetSpan) :
            filter.Convert(ref reader, ref targetSpan);
        input.Advance((int) reader.RelativePosition);
        return target.Length - targetSpan.Length;

    }
    ///<inheritdoc/>
    public void Dispose()
    {
        input.Dispose();
        (filter as IDisposable)?.Dispose();
        (inner as IDisposable)?.Dispose();
    }
}
