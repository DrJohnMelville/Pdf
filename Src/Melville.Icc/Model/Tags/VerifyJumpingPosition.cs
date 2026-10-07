using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

internal static class VerifyJumpingPosition
{
    [Conditional("DEBUG")]
    public static void VerifyInCorrectPositionForTagRelativeOffsets(
        ref this SequenceReader<byte> reader) =>
        Debug.Assert(reader.Sequence.Slice(reader.Sequence.Start, reader.Position).Length == 4);
    [Conditional("DEBUG")]
    public static void VerifyInCorrectPositionForTagRelativeOffsets(
        ref this SpanParsingBuffer reader) =>
        Debug.Assert(reader.RelativePosition == 4);

    public static SequenceReader<byte> ReaderAt(
        in this SequenceReader<byte> reader, uint position, uint length) =>
        new(reader.Sequence.Slice(position, length));

    public static SequenceReader<byte> ReaderAt(in this SequenceReader<byte> reader, uint position) =>
        new(reader.Sequence.Slice(position));

    public static SpanParsingBuffer ReaderAt(in this SpanParsingBuffer reader, uint position, 
        uint length = uint.MaxValue)
    {
        var delta = position - reader.RelativePosition;
        var span = reader.Peek()[(int)delta..];
        if (length < span.Length) span = span[..(int)length];
        return new SpanParsingBuffer(span);
    }

    public static SequenceReader<byte> ReadPositionNumber(ref this SequenceReader<byte> reader) =>
        reader.ReaderAt(reader.GetUInt32BigEndian(), reader.GetUInt32BigEndian());
    public static SpanParsingBuffer ReadPositionNumber(ref this SpanParsingBuffer reader) =>
        reader.ReaderAt(reader.GetUInt32BigEndian(), reader.GetUInt32BigEndian());
}