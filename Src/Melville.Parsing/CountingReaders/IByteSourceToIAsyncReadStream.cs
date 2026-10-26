using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Melville.Parsing.CountingReaders;

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
}
