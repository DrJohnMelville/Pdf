using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
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
    private readonly ClosedInterval[] outputIntervals; 
    protected int MaxValue { get; }
    private ClosedInterval sourceInterval;
    
    public ByteWriter(IColorSpace colorSpace, ClosedInterval[] outputIntervals, int maxValue)
    {
        this.colorSpace = colorSpace;
        this.outputIntervals = outputIntervals;
        MaxValue = maxValue;
        partialColor = new double[colorSpace.ExpectedComponents];
        CheckOutputIntervalLength(outputIntervals);
        sourceInterval = new ClosedInterval(0, MaxValue);
    }

    private void CheckOutputIntervalLength(ClosedInterval[] outputIntervals)
    {
        if (partialColor.Length != outputIntervals.Length)
            throw new PdfParseException("Incorrect number of output intervals");
    }

    public abstract unsafe void WriteBytes(
        ref SequenceReader<byte> input, ref byte* output, byte* nextPos);

    protected unsafe void PushComponent(
        ref byte* output, int numerator) =>
        PushComponent(ref output, (float)sourceInterval.MapTo(outputIntervals[nextComponent], numerator));

    private unsafe void PushComponent(ref byte* output, float component)
    {
        partialColor[nextComponent++] = component;
        if (nextComponent == partialColor.Length)
            PushPixel(ref output);
    }

    private unsafe void PushPixel(ref byte* output)
    {
        var color = colorSpace.SetColor(partialColor);
        BitmapPointerMath.PushPixel(ref output, color.AsPreMultiplied());
        nextComponent = 0;
    }
    public abstract int MinimumInputSize { get; }
}

public static class BitmapPointerMath
{
    public static unsafe void PushPixel(ref byte* output, in DeviceColor color)
    {
        *output++ = color.BlueByte;
        *output++ = color.GreenByte;
        *output++ = color.RedByte;
        *output++ = color.Alpha;
    }
}