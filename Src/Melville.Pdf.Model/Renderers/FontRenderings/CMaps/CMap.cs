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
                    int outputlen = byteRange.WriteMapping(character, scratchBuffer);
                    #warning need to handle scratch buffer overflow
                    return scratchBuffer[..outputlen];
                }
            }
        }

        bytesConsumed = minByteLen;
        scratchBuffer.Span[0] = 0;
        return scratchBuffer[..1];
    }
}