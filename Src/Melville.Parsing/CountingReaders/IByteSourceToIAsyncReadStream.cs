using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace Melville.Parsing.CountingReaders;

///// <summary>
///// this is a temporary shim to change a IByteSource into an IStreamReader.  It lets me try
///// some new parsing from the old infrasructure.
///// </summary>
//[Obsolete("This is a temporary shim for replacing the IByteSource with the ParsingBuffer")]
//public class IByteSourceToIAsyncReadStream(IByteSource source) : IAsyncReader
//{
//    long startPos = source.Position;
//    ///<inheritdoc/>
//    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
//    {
//        var positionDelta = startPos - source.Position;
//        var result = await source.ReadAsync();
//        var ret = TransferBuffer(result.Buffer.Slice(positionDelta), buffer.Span);
//        source.AdvanceTo(result.Buffer.Start, result.Buffer.GetPosition(startPos - source.Position));
//        return (int)ret;
//    }
//    long TransferBuffer(ReadOnlySequence<byte> buffer, Span<byte> span)
//    {
//        var retLen = Math.Min(buffer.Length, span.Length);
//        buffer.Slice(0, retLen).CopyTo(span);
//        startPos += retLen;
//        return retLen;
//    }
//}

#warning need to get rid of the sequence readers as well.
/// <summary>
/// Transitional shim object
/// </summary>
public static class SequenceReaderShim
{
    /// <summary>
    /// Transitional shim object
    /// </summary>
    [Obsolete("This is a shim for the reader transition")]
    public static SpanParsingBuffer AsSpanParsingBuffer(this SequenceReader<byte> reader) =>
        reader.UnreadSequence.AsSpanParsingBuffer();

    /// <summary>
    /// Transitional shim object
    /// </summary>
    [Obsolete("This is a shim for the reader transition")]
    public static SpanParsingBuffer AsSpanParsingBuffer(this ReadOnlySequence<byte> reader)
    {
        var buffer = new byte[reader.Length];
        reader.CopyTo(buffer);
        return new(buffer);

    }
    [Obsolete]
    public static async ValueTask<T> TempBridgeAsync<T>(this PipeReader reader, Func<ParsingBuffer, ValueTask<T>> action)
    {
        var stream = new ReaderFromPipe(reader);
        using var buffer = new ParsingBuffer(stream);
        var ret = await action(buffer).CA();
        var result = await reader.ReadAsync();
        reader.AdvanceTo(result.Buffer.GetPosition(buffer.RelativePosition));
        return ret;
    }
    [Obsolete]
    public static async ValueTask TempBridgeAsync(
        this PipeReader reader, Func<ParsingBuffer, ValueTask> action)
    {
        var stream = new ReaderFromPipe(reader);
        using var buffer = new ParsingBuffer(stream);
        await action(buffer).CA();
        var result = await reader.ReadAsync();
        reader.AdvanceTo(result.Buffer.GetPosition(buffer.RelativePosition));
    }
}

public class ReaderFromPipe(PipeReader source) : IAsyncReader
{
    SequencePosition currentPosition;
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var result = await source.ReadAsync();
        if (currentPosition.GetObject() is null) currentPosition = result.Buffer.Start;
        var sourceSeq = result.Buffer.Slice(currentPosition);
        if (sourceSeq.Length > buffer.Length) sourceSeq = sourceSeq.Slice(0, buffer.Length);
        sourceSeq.CopyTo(buffer.Span);
        source.AdvanceTo(result.Buffer.Start, sourceSeq.End);
        return (int)sourceSeq.Length;
    }
}