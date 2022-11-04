using System.Buffers;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class NBitByteWriter : ByteWriter
{
    private int bits;

    public NBitByteWriter(IComponentWriter writer, int bits) : 
        base(MaxValueForBits(bits), writer)
    {
        this.bits = bits;
    }

    private static byte MaxValueForBits(int bits) => (byte)((1 << bits) - 1);

    public override unsafe void WriteBytes(
        scoped ref SequenceReader<byte> input, scoped ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryRead(out var readVal))
        {
            PushSingleByte(ref output, nextPos, readVal);
        }
    }

    private unsafe void PushSingleByte(ref byte* output, byte* nextPos, byte readVal)
    {
        int bitsLeft = 8;
        while (bitsLeft > 0 && output < nextPos)
        {
            PushComponent(ref output, (readVal >> (bitsLeft - bits)) & MaxValue);
            bitsLeft -= bits;
        }
    }
    public override int MinimumInputSize => 1;
}