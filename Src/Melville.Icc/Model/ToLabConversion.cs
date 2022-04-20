using Melville.Icc.Model.Tags;
using Melville.INPC;

namespace Melville.Icc.Model;

public partial class ToLabConversion : IColorTransform
{
    [DelegateTo]private readonly IColorTransform innerTransform;

    public ToLabConversion(IColorTransform innerTransform)
    {
        this.innerTransform = innerTransform;
    }

    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        innerTransform.Transform(input, output);
        output[0] *= 100f;
        output[1] = AbTransform(output[1]);
        output[2] = AbTransform(output[2]);
    }

    private static float AbTransform(float a) => (a * 256f) - 128f;
}
public partial class FromLabConversion : IColorTransform
{
    [DelegateTo]private readonly IColorTransform innerTransform;

    public FromLabConversion(IColorTransform innerTransform)
    {
        this.innerTransform = innerTransform;
    }

    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Span<float> transformedLab = 
            stackalloc float[] { input[0] / 100f, AbTransform(input[1]), input[2] + 0.5f };
        innerTransform.Transform(transformedLab, output);
    }

    private static float AbTransform(float value) => (value + 128f)/256f;
}