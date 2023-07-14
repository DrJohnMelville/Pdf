using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Appends a CR to the end of a sequence reader, which means the rest of the parser
/// does not have to hand end of stream conditions.
/// </summary>
public static class AppendToSequenceReader
{
    /// <summary>
    /// Append a memory to a ReadOnlySequence
    /// </summary>
    /// <typeparam name="T">The base tpe of the sequence</typeparam>
    /// <param name="seq">The sequence to come first.</param>
    /// <param name="mem">The memory to append</param>
    /// <returns>A sequence compose of the original sequence followed by the Memory</returns>
    public static ReadOnlySequence<T> Append<T>(this ReadOnlySequence<T> seq, Memory<T> mem)
    {
        var builder = new ReadOnlySequenceBuilder<T>();
        builder.Append(seq);
        builder.Append(mem);
        return builder.GetSequence();
    }

    private static readonly byte[] terminalWhitespace = { (byte)'\r' };

    /// <summary>
    /// Append a carriage return to the given sequence
    /// </summary>
    /// <param name="seq">The sequence to append to</param>
    /// <returns>The A new sequence composed of the old sequenct plus a carriage return</returns>
    public static ReadOnlySequence<byte> AppendCR(this ReadOnlySequence<byte> seq) => 
        seq.Append(terminalWhitespace);

    /// <summary>
    /// Creates a read only sequence composed of the memory plus a carriage return
    /// </summary>
    /// <param name="mem">The source memory</param>
    public static ReadOnlySequence<byte> AppendCR(this Memory<byte> mem)
    {
        var builder = new ReadOnlySequenceBuilder<byte>();
        builder.Append(mem);
        builder.Append(terminalWhitespace);
        return builder.GetSequence();

    }
}