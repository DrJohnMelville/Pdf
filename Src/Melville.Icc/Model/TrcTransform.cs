using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;

namespace Melville.Icc.Model;

public class TrcTransform : IColorTransform
{
    private readonly Matrix3x3 matrix;
    private readonly ICurveTag rTrc;
    private readonly ICurveTag gTrc;
    private readonly ICurveTag bTrc;

    public TrcTransform(in Matrix3x3 matrix, ICurveTag rTrc, ICurveTag gTrc, ICurveTag bTrc)
    {
        this.matrix = matrix;
        this.rTrc = rTrc;
        this.gTrc = gTrc;
        this.bTrc = bTrc;
    }

    public int Inputs => 3;
    public int Outputs => 3;
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        output[0] = rTrc.Evaluate(input[0]);
        output[1] = gTrc.Evaluate(input [1]);
        output[2] = bTrc.Evaluate(input[2]);
        matrix.PostMultiplyBy(output, output);
    }
}