using System;
using System.Buffers;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal class CompositeCmap(IReadCharacter outer, IReadCharacter inner): IReadCharacter
{
    /// <inheritdoc />
    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, 
        out int bytesConsumed)
    {
        var charBuffer = ArrayPool<uint>.Shared.Rent(16);
        var intermediate = outer.GetCharacters(input, charBuffer, 
            out bytesConsumed).Span;
        var bytes = AsBigEndianBytes(intermediate);
        var characters = inner.GetCharacters(
            bytes, scratchBuffer, out _);
        ArrayPool<uint>.Shared.Return(charBuffer);
        return characters;
    }

    private static Memory<byte> AsBigEndianBytes(
        ReadOnlySpan<uint> intermediate)
    {
        // length is almost always 1, and the performance of allocating
        // small arrays is very competitive with rental
        var byteBuffer = new byte[2 * intermediate.Length];
        for (int i = 0; i < intermediate.Length; i++)
        {
            byteBuffer[2*i] = (byte)(intermediate[i]>>8);
            byteBuffer[2*i + 1] = (byte)(intermediate[i] & 0xFF);
        }
        return byteBuffer.AsMemory();
    }
}