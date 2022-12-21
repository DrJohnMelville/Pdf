using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

internal static class CurveTagParser
{
    public static ICurveTag Parse(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var length = (int)reader.ReadBigEndianUint32();
        return length switch
        {
            0 => NullCurve.Instance,
            1 => ExponentialFunction(reader.Readu8Fixed8()),
            _ => CreateCurveSegment(reader.ReadScaledFloatArray(length, 2))
        };
    }

    private static ParametricCurveTag ExponentialFunction(float exponent) =>
        new(exponent, 1, 0, 0, float.MinValue, 1, 0);

    private static ICurveTag CreateCurveSegment(float[] points)
    {
        var ret = new SampledCurveSegment(points);
        ((ICurveSegment)ret).Initialize(0,1,points[0]);
        return ret;
    }
}