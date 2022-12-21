using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

internal class BlackTransform : IColorTransform
{
    private readonly XyzNumber whitePoint;
    private readonly ICurveTag kTrc;

    public BlackTransform(XyzNumber whitePoint, ICurveTag kTrc)
    {
        this.whitePoint = whitePoint;
        this.kTrc = kTrc;
    }

    public int Inputs => 1;
    public int Outputs => 3;
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        var kValue = kTrc.Evaluate(input[0]);
        output[0] = kValue * whitePoint.X;
        output[1] = kValue * whitePoint.Y;
        output[2] = kValue * whitePoint.Z;
    }
}