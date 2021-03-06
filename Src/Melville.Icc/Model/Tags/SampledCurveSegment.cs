using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

public class SampledCurveSegment : ICurveSegment
{
    public float Minimum { get; private set; }
    public float Maximum { get; private set; }
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
        Maximum = maximum;
        samples[0] = valueAtMinimum;
    }

    public float Evaluate(float input) => 
        Interpolation.InterpolateFloatArray(new ReadOnlySpan<float>(samples), Minimum, Maximum, input);
}