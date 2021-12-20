using System.Buffers;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public class NBitByteWriter : ByteWriter
{
    private int bits;

    public NBitByteWriter(IColorSpace colorSpace, ClosedInterval[] outputIntervals, int bits) : 
        base(colorSpace, outputIntervals, MaxValueForBits(bits))
    {
        this.bits = bits;
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
            PushComponent(ref output, (readVal >> (bitsLeft - bits)) & MaxValue);
            bitsLeft -= bits;
        }
    }
    
    public override int MinimumInputSize => 1;

}