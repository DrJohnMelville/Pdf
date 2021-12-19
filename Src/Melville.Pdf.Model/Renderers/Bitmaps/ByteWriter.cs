using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IByteWriter
{
    unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, 
         byte* nextPos);
}

public abstract class ByteWriter: IByteWriter
{
    private readonly IColorSpace colorSpace;
    private readonly double[] partialColor;
    private int nextComponent = 0;
    
    public ByteWriter(IColorSpace colorSpace)
    {
        this.colorSpace = colorSpace;
        partialColor = new double[colorSpace.ExpectedComponents];
    }

    public abstract unsafe void WriteBytes(
        ref SequenceReader<byte> input, ref byte* output, byte* nextPos);
    
    protected unsafe void PushComponent(ref byte* output, float component)
    {
        partialColor[nextComponent++] = component;
        if (nextComponent == partialColor.Length)
            PushPixel(ref output);
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

public class ByteWriter16 : ByteWriter
{
    private const float MaxValue = (1 << 16) - 1;
    public ByteWriter16(IColorSpace colorSpace) : base(colorSpace)
    {
    }

    public override unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, byte* nextPos)
    {
        while (output < nextPos && input.TryPeek(out var high) && input.TryPeek(1, out var low))
        {
            input.Advance(2);
            PushComponent(ref output, ((high << 8)|low)/MaxValue);
        }
    }
}