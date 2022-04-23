using System;
using Melville.Icc.Model.Tags;

namespace Melville.Pdf.Model.Renderers.Colors;

public static class ChromaticAdaptation
{
    private static readonly Matrix3x3 BradfordForward = new(
        0.8951000f, 0.2664000f, -0.1614000f,
        -0.7502000f, 1.7135000f, 0.0367000f,
        0.0389000f, -0.0685000f, 1.0296000f
    );

    private static readonly Matrix3x3 BradfordInverse = new(
        0.9869929f, -0.1470543f, 0.1599627f,
        0.4323053f, 0.5183603f, 0.0492912f,
        -0.0085287f, 0.0400428f, 0.9684867f
    );

    //Re2turns a matrix that converts xyz coordingats from a source whitepoint to dest.
    public static Matrix3x3 AdaptationMatrix(in FloatColor source, in FloatColor dest)
    {
        Span<float> sourceSpan = stackalloc float[] { source.Red, source.Green, source.Blue };
        Span<float> destSpan = stackalloc float[] { dest.Red, dest.Green, dest.Blue };
        BradfordForward.PostMultiplyBy(sourceSpan, sourceSpan);
        BradfordForward.PostMultiplyBy(destSpan, destSpan);
        return BradfordInverse * new Matrix3x3(
                                   destSpan[0] / sourceSpan[0], 0, 0,
                                   0, destSpan[1] / sourceSpan[1], 0,
                                   0, 0, destSpan[2] / sourceSpan[2])
                               * BradfordForward;
    }
}