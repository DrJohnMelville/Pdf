using System.Buffers;
using System.Diagnostics;

namespace Melville.Icc.Model.Tags;

public static class VerifyJumpingPosition
{
    [Conditional("DEBUG")]
    public static void VerifyInCorrectPositionForTagRelativeOffsets(
        ref this SequenceReader<byte> reader) => 
        Debug.Assert(reader.Sequence.Slice(reader.Sequence.Start, reader.Position).Length == 4);

    public static SequenceReader<byte> ReaderAt(
        in this SequenceReader<byte> reader, uint position, uint length) =>
        new(reader.Sequence.Slice(position, length)); 
    
    public static SequenceReader<byte> ReaderAt(in this SequenceReader<byte> reader, uint position) =>
        new(reader.Sequence.Slice(position));
}