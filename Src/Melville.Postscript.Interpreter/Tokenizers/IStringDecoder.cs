using System;
using System.Buffers;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Represents string decoding strategies
/// </summary>
public interface IStringDecoder<TState>
{
    /// <summary>
    /// Decode a number of bytes from the CodeSource to the destination
    /// </summary>
    /// <param name="source">CodeSource of bytes to decode</param>
    /// <param name="destination">span to write to</param>
    /// <param name="state">A state variable that will be maintained over successive calls
    /// in the same stream</param>
    /// <returns>positive number of bytes written, 0 if stream is ended,
    /// -1 if more bytes are needed.</returns>
    public int DecodeFrom(
        ref SequenceReader<byte> source, scoped Span<byte> destination, ref TState state);

    /// <summary>
    /// Gets the maximum number of characters that could be written in a single call
    /// to DecodeFrom
    /// </summary>
    int MaxCharsPerBlock { get; }
}