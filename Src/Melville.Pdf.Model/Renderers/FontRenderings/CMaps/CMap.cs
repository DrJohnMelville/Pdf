using System;
using System.Collections.Generic;
using System.Linq;
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

    public Span<uint> GetCharacters(
        in ReadOnlySpan<byte> input, in Span<uint> scratchBuffer, out int bytesConsumed)
    {
        VariableBitChar character = new();
        bytesConsumed = 0;
        for (var i = 0; i < maxByteLen; i++)
        {
            if (bytesConsumed >= input.Length) return scratchBuffer[..0];
            character = character.AddByte(input[bytesConsumed++]);
            foreach (var byteRange in byteRanges)
            {
                if (byteRange.AppliesTo(character))
                {
                    var outputBuffer = scratchBuffer;
                    while (true) {
                        if (byteRange.WriteMapping(character, outputBuffer) is var bytesWritten and >= 0)
                        {
                            return outputBuffer[..bytesWritten];
                        }
                        if (scratchBuffer.Length >= 128) break;
                        outputBuffer = new uint[outputBuffer.Length*2].AsSpan();
                    }
                }
            }
        }

        bytesConsumed = minByteLen;
        if (scratchBuffer.Length == 0) return new uint[] { 0 };
        scratchBuffer[0] = 0;
        return scratchBuffer[..1];
    }
}