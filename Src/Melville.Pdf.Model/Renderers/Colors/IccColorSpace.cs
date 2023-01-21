using System;
using System.Linq;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

internal class IccColorSpace : IColorSpace
{
    private readonly IColorTransform transform;

    public IccColorSpace(IColorTransform transform)
    {
        this.transform = transform;
    }

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length < transform.Inputs) // for now ignore extra parameters for transparency
            throw new PdfParseException("Incorrect number of color parameters");
        Span<float> inputs = stackalloc float[newColor.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = (float)newColor[i];
        }

        Span<float> output = stackalloc float[3];
        transform.Transform(inputs, output);
        return DeviceColor.FromDoubles(output[0], output[1], output[2]);
    }

    public virtual DeviceColor DefaultColor()
    {
        // stackalloc will initialize the array to all 0s which is what the spec requires.
        return SetColor(stackalloc double[transform.Inputs]);
    }

    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);

    public int ExpectedComponents => transform.Inputs;
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) => 
      Enumerable.Repeat(new ClosedInterval(0, 1), ExpectedComponents).ToArray();
}

internal class IccColorspaceWithBlackDefault : IccColorSpace
{
    public IccColorspaceWithBlackDefault(IColorTransform transform) : base(transform)
    {
    }

    public override DeviceColor DefaultColor() => DeviceColor.Black;
}