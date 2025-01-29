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
    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed)
    {
        if (scratchBuffer.Length < 1)
        {
            bytesConsumed = -1;
            return scratchBuffer[..0];
        }
        bytesConsumed = 2;
        var inputSpan = input.Span;
        scratchBuffer.Span[0]  = (uint)(inputSpan[0] << 8) | inputSpan[1];
        return scratchBuffer[..1];
    }
}
