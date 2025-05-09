using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

/// <summary>
/// This class os a character reader that assumes all characters are a single byte with no mapping.
/// </summary>
[StaticSingleton()]
public sealed partial class SingleByteCharacters : IReadCharacter
{
    /// <inheritdoc />
    public Span<uint> GetCharacters(
        in ReadOnlySpan<byte> input, in Span<uint> scratchBuffer, out int bytesConsumed)
    {
        if (scratchBuffer.Length < 1)
        {
            bytesConsumed = -1;
            return scratchBuffer[..0];
        }
        bytesConsumed = 1;
        scratchBuffer[0] = input[0];
        return scratchBuffer[..1];
    }
}