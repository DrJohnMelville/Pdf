using System.Buffers;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class NBitByteWriter(ComponentWriter writer, int bits, int lineWidth) : 
    ByteWriter(MaxValueForBits(bits), writer)
{
    private static byte MaxValueForBits(int bits) => (byte)((1 << bits) - 1);
    int currentCol = 0;

    public override unsafe void WriteBytes(
        scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryRead(out var readVal))
        {
            PushSingleByte(ref output, nextPos, readVal);
            currentCol %= lineWidth;
        }
    }

    private unsafe void PushSingleByte(ref byte* output, byte* nextPos, byte readVal)
    {
        for (var bitsLeft = 8;  
            bitsLeft > 0 && output < nextPos && currentCol < lineWidth;
            bitsLeft -= bits, currentCol++)
        {
            PushComponent(ref output, (readVal >> (bitsLeft - bits)) & MaxValue);
        }
    }
}