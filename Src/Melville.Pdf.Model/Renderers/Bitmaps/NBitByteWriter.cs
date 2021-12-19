using System;
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
            PushComponent(ref output, ((readVal >> (bitsLeft - bits)) & mask),  mask);
            bitsLeft -= bits;
        }
    }
    
    public override int MinimumInputSize => 1;

}

public class FastBitmapWriterRGB8: IByteWriter
{
    public unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (input.Remaining >= 3 && output < nextPos)
        {
            var segment = new Span<byte>(output, 4);
            input.TryRead(out *(output + 2));
            input.TryRead(out *(output + 1));
            input.TryRead(out *(output));
            output += 3;
            *output++ = 0xFF;
        }
    }
    public int MinimumInputSize => 3;
}