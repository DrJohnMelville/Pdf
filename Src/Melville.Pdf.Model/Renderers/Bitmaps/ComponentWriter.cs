
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IComponentWriter
{
    unsafe void WriteComponent(ref byte* target, int component);
}

public class ComponentWriter : IComponentWriter
{
    private readonly ClosedInterval sourceInterval;
    private readonly ClosedInterval[] outputIntervals;
    private readonly IColorSpace colorSpace;
    private readonly double[] partialColor;
    private int nextComponent = 0;

    public ComponentWriter(ClosedInterval sourceInterval, ClosedInterval[] outputIntervals, IColorSpace colorSpace)
    {
        this.sourceInterval = sourceInterval;
        this.outputIntervals = outputIntervals;
        this.colorSpace = colorSpace;
        CheckOutputIntervalLength(outputIntervals, colorSpace);
        partialColor = new double[outputIntervals.Length];
    }

    private static void CheckOutputIntervalLength(ClosedInterval[] outputIntervals, IColorSpace colorSpace)
    {
        if (outputIntervals.Length != colorSpace.ExpectedComponents)
            throw new PdfParseException("Incorrect number of output intervals");
    }

    public unsafe void WriteComponent(ref byte* target, int component)
    {
        PushComponent(ref target, (float)sourceInterval.MapTo(outputIntervals[nextComponent],
            component));
    }
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

}