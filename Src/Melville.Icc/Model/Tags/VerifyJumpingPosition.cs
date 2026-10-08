using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

internal static class VerifyJumpingPosition
{
    [Conditional("DEBUG")]
    public static void VerifyInCorrectPositionForTagRelativeOffsets(
        ref this SpanParsingBuffer reader) =>
        Debug.Assert(reader.RelativePosition == 4);

    public static SpanParsingBuffer ReaderAt(in this SpanParsingBuffer reader, uint position, 
        uint length = uint.MaxValue)
    {
        var delta = position - reader.RelativePosition;
        var span = reader.Peek()[(int)delta..];
        if (length < span.Length) span = span[..(int)length];
        return new SpanParsingBuffer(span);
    }

    public static SpanParsingBuffer ReadPositionNumber(ref this SpanParsingBuffer reader) =>
        reader.ReaderAt(reader.GetUInt32BigEndian(), reader.GetUInt32BigEndian());
}