using System.Buffers;

namespace Melville.Parsing.SequenceReaders;

public static class ReadOnlySequenceExtensions
{
    public static ReadOnlySequence<T> Slice<T>(this ReadOnlySequence<T> seq, Range index)
    {
        var (offset, length) = index.GetOffsetAndLength((int)seq.Length);
        return seq.Slice(offset, length);
    }
       
}