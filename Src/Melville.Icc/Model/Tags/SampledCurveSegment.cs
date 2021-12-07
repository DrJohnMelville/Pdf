using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class SampledCurveSegment : ICurveSegment
{
    public float Minimum { get; private set; }
    public float Delta { get; private set; }
    private float[] samples;
    public IReadOnlyList<float> Samples => samples;

    public SampledCurveSegment(params float[] samples)
    {
        this.samples = samples;
    }
    public SampledCurveSegment(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        samples = reader.ReadIEEE754FloatArray((int)reader.ReadBigEndianUint32(), 1);
    }

    public void Initialize(float minimum, float maximum, float valueAtMinimum)
    {
        Minimum = minimum;
        Delta = (maximum - minimum) / (samples.Length - 1);
        samples[0] = valueAtMinimum;
    }

    public float Evaluate(float input)
    {
        var index = (input - Minimum) / Delta;
        var bottom = Math.Floor(index);
        var intBottom = (int)bottom;
        if (IsLastPointSpecialCase(intBottom)) return samples[intBottom];
        var subdelta = index - bottom;
        return Interpolation.InterpolateFraction(subdelta, samples[intBottom], samples[intBottom + 1]);
    }

    private bool IsLastPointSpecialCase(int intBottom) => intBottom + 1 == samples.Length;
}