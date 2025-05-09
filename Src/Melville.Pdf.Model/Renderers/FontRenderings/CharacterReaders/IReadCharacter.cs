using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

/// <summary>
///  Read characters from a source string in a specific encoding.
/// </summary>
public interface IReadCharacter
{
    /// <summary>
    /// Get a set of characters from the input.  If the font uses a cmap, an arbitrary number
    /// of input bytes could map to an arbitrary number of characters.
    /// </summary>
    /// <param name="input">the input bytes</param>
    /// <param name="scratchBuffer">A buffer that can be reused to hold the output if it
    /// is big enough</param>
    /// <param name="bytesConsumed">Receives the number of bytes consumed by the operation</param>
    /// <returns>A memory containing the character(s) read from the input.  This can be but
    /// is not required to be a portion of the scratchBuffer</returns>
    Span<uint> GetCharacters(
        in ReadOnlySpan<byte> input, in Span<uint> scratchBuffer, out int bytesConsumed);
}

/// <summary>
/// Helper method for IReadCharacter
/// </summary>
public static class ReadCharacterOperations
{
    /// <summary>
    /// This read a character from the input, and advances the input beyond the read character
    /// </summary>
    /// <param name="target">The strategy to get a character from the input</param>
    /// <param name="input">The input</param>
    /// <param name="scratchBuffer">A buffer that may be used to hold the output.</param>
    /// <returns>A memory containing the character(s) read from the input.  This can be but
    /// is not required to be a portion of the scratchBuffer</returns>
    public static Memory<uint> GetCharacters(this IReadCharacter target,
        ref ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer)
    {
        var ret = target.GetCharacters(input.Span, scratchBuffer.Span, out var bytesConsumed);
        input = input[bytesConsumed..];
        var retMemory = scratchBuffer[..ret.Length];
        return ret.SequenceEqual(retMemory.Span)?retMemory:ret.ToArray().AsMemory();
    }
}