using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

[StaticSingleton]
internal sealed partial class TwoByteCharacters : IReadCharacter
{
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => 
        ((uint)(input[0] << 8)|input[1], 2);

    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed)
    {
        bytesConsumed = 2;
        var inputSpan = input.Span;
        scratchBuffer.Span[0]  = (uint)(inputSpan[0] >> 8) | inputSpan[1];
        return scratchBuffer[..1];
    }
}
