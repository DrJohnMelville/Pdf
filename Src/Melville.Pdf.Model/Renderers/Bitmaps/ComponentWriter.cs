using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class ComponentWriter 
{
    private readonly ClosedInterval sourceInterval;
    private readonly ClosedInterval[] outputIntervals;
    private readonly IColorSpace colorSpace;

    public ComponentWriter(ClosedInterval sourceInterval, ClosedInterval[] outputIntervals, IColorSpace colorSpace)
    {
        this.sourceInterval = sourceInterval;
        this.outputIntervals = outputIntervals;
        this.colorSpace = colorSpace;
        CheckOutputIntervalLength(outputIntervals, colorSpace);
    }

    public int ColorComponentCount => outputIntervals.Length;

    private static void CheckOutputIntervalLength(ClosedInterval[] outputIntervals, IColorSpace colorSpace)
    {
        if (outputIntervals.Length != colorSpace.ExpectedComponents)
            throw new PdfParseException("Incorrect number of output intervals");
    }

    public unsafe DeviceColor ColorFromComponents(int[] component)
    {
        Span<double> partialColor = stackalloc double[component.Length];
        for (int i = 0; i < partialColor.Length; i++)
        {
            partialColor[i] = sourceInterval.MapTo(outputIntervals[i], component[i]);
        }

        return colorSpace.SetColor(partialColor);
    }

}