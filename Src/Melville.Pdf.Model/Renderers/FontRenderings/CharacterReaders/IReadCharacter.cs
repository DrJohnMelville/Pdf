using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

/// <summary>
///  Read characters from a source string in a specific encoding.
/// </summary>
public interface IReadCharacter
{
    /// <summary>
    /// Get a character from a byte string
    /// </summary>
    (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input);
}