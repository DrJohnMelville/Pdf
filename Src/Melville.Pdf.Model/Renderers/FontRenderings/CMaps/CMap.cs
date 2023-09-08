using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Linq;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal class CMap: IReadCharacter
{
    private readonly IList<ByteRange> byteRanges;
    private readonly int minByteLen;
    private readonly int maxByteLen;

    internal CMap(IList<ByteRange> byteRanges)
    {
        this.byteRanges = byteRanges;
        (minByteLen,maxByteLen) = byteRanges
            .Select(i => i.ByteLength())
            .MinMax();
    }

    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed)
    {
        VariableBitChar character = new();
        bytesConsumed = 0;
        for (var i = 0; i < maxByteLen; i++)
        {
            if (bytesConsumed >= input.Length) return scratchBuffer[..0];
            character = character.AddByte(input.Span[bytesConsumed++]);
            foreach (var byteRange in byteRanges)
            {
                if (byteRange.AppliesTo(character))
                {
                    return RenderToBuffer(scratchBuffer, byteRange, character);
                }
            }
        }

        bytesConsumed = minByteLen;
        if (scratchBuffer.Length == 0) return new uint[] { 0 };
        scratchBuffer.Span[0] = 0;
        return scratchBuffer[..1];
    }

    private static Memory<uint> RenderToBuffer(
        Memory<uint> scratchBuffer, ByteRange byteRange, in VariableBitChar character)
    {
        return byteRange.WriteMapping(character, scratchBuffer) >= 0 ? 
            scratchBuffer[..byteRange.WriteMapping(character, scratchBuffer)] : 
            RenderToBuffer(new uint[Math.Max(128, 2 * scratchBuffer.Length)], byteRange, character);
    }
}