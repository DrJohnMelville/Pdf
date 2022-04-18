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
        output[0] *= 100;
        output[1] -= 0.5f;
        output[2] -= 0.5f;
    }
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
            stackalloc float[] { input[0] / 100f, input[1] + 0.5f, input[2] + 0.5f };
        innerTransform.Transform(transformedLab, output);
    }
}