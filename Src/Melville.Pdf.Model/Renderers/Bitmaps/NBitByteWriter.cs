using System.Buffers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class NBitByteWriter : ByteWriter
{
    private int bits;
    private byte mask;

    public NBitByteWriter(IColorSpace colorSpace, int bits) : base(colorSpace)
    {
        this.bits = bits;
        mask = MaxValueForBits(bits);
    }

    private static byte MaxValueForBits(int bits) => (byte)((1 << bits) - 1);

    public override unsafe void WriteBytes(
        ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
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
            PushComponent(ref output, ((float)((readVal >> (bitsLeft - bits)) & mask)) / mask);
            bitsLeft -= bits;
        }
    }
    
}