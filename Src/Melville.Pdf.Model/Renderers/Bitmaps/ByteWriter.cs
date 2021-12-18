using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IByteWriter
{
    unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, 
         byte* nextPos);
}

public class ByteWriter: IByteWriter
{
    private readonly IColorSpace colorSpace;
    private readonly double[] partialColor;
    private int nextComponent = 0;
    private int bits;
    private byte mask;

    public ByteWriter(IColorSpace colorSpace, int bits)
    {
        this.colorSpace = colorSpace;
        partialColor = new double[colorSpace.ExpectedComponents];
        this.bits = bits;
        mask = (byte)((1 << bits) - 1);
    }

    public unsafe void WriteBytes(
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
            partialColor[nextComponent++] =
                ((float)((readVal>>(bitsLeft-bits)) & mask)) / mask;
            bitsLeft -= bits;
            if (nextComponent == partialColor.Length)
                PushPixel(ref output);
        }
    }

    private unsafe void PushPixel(ref byte* output)
    {
        var color = colorSpace.SetColor(partialColor);
        *output++ = color.BlueByte;
        *output++ = color.GreenByte;
        *output++ = color.RedByte;
        *output++ = 0xFF;
        nextComponent = 0;
    }
}