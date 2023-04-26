using System.Buffers;

namespace Melville.Parsing.SequenceReaders;

/// <summary>
/// Implements a slice method on readonlySequence
/// </summary>
public static class ReadOnlySequenceExtensions
{
    /// <summary>
    /// Slice a ReadOnlySequence using a range variable.
    /// </summary>
    /// <param name="seq">The sequence to be sliced.</param>
    /// <param name="index">Range indicating the desired subset of the sequence.</param>
    /// <typeparam name="T">Element type of the sequence.</typeparam>
    /// <returns>A ReadOnlySequence comprising the subset of the source sequence indicated by the index</returns>
    public static ReadOnlySequence<T> Slice<T>(this ReadOnlySequence<T> seq, Range index)
    {
        var (offset, length) = index.GetOffsetAndLength((int)seq.Length);
        return seq.Slice(offset, length);
    }
       
}