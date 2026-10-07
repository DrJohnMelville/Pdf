using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Melville.Parsing.CountingReaders;

/// <summary>
/// this is a temporary shim to change a IByteSource into an IStreamReader.  It lets me try
/// some new parsing from the old infrasructure.
/// </summary>
[Obsolete("This is a temporary shim for replacing the IByteSource with the ParsingBuffer")]
public class IByteSourceToIAsyncReadStream(IByteSource source) : IAsyncReader
{
    ///<inheritdoc/>
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var result = await source.ReadAsync();
        var ret = TransferBuffer(result.Buffer, buffer.Span);
        source.AdvanceTo(result.Buffer.GetPosition(ret));
        return (int)ret;
    }
    long TransferBuffer(ReadOnlySequence<byte> buffer, Span<byte> span)
    {
        var retLen = Math.Min(buffer.Length, span.Length);
        buffer.Slice(0, retLen).CopyTo(span);
        return retLen;
    }
}

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

    /// <summary>
    /// Transitional shim object
    /// </summary>
    [Obsolete("This is a shim for the reader transition")]
    public static IAsyncReader AsAsyncReader(IByteSource src) => new IByteSourceToIAsyncReadStream(src);
}
