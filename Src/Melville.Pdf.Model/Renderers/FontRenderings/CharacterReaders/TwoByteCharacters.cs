using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

/// <summary>
/// This is an IReadCharacter that reads two byte big endian characters, consistent with
/// Unicode-16BE
/// </summary>
[StaticSingleton]
public sealed partial class TwoByteCharacters : IReadCharacter
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
        bytesConsumed = 2;
        scratchBuffer[0]  = (uint)(input[0] << 8) | input[1];
        return scratchBuffer[..1];
    }
}
