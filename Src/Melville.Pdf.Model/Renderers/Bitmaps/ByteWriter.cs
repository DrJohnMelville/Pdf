using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IByteWriter
{
    unsafe void WriteBytes(ref SequenceReader<byte> input, ref byte* output, 
         byte* nextPos);
    int MinimumInputSize { get; }
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

    protected unsafe virtual void PushComponent(
        ref byte* output, int numerator, int denominator) =>
        PushComponent(ref output, ((float)numerator) / denominator);  
    
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

    public abstract int MinimumInputSize { get; }
}

public class IntegerComponentByteWriter : NBitByteWriter
{
    public IntegerComponentByteWriter(IColorSpace colorSpace, int bits) : base(colorSpace, bits)
    {
    }

    protected override unsafe void PushComponent(ref byte* output, int numerator, int denominator)
    {
        PushComponent(ref output, numerator);
    }
}