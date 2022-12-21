using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// An ICC curve segment defined by an array of samples and linear interpolation between samples
/// </summary>
public class SampledCurveSegment : ICurveSegment
{
    /// <summary>
    /// Minimum for the domain of the function
    /// </summary>
    public float Minimum { get; private set; }
    /// <summary>
    /// Maximum extent of the domain of the function
    /// </summary>
    public float Maximum { get; private set; }
    private float[] samples;
    /// <summary>
    /// Sample data that defines the function
    /// </summary>
    public IReadOnlyList<float> Samples => samples;

    internal SampledCurveSegment(params float[] samples)
    {
        this.samples = samples;
    }
    internal SampledCurveSegment(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        samples = reader.ReadIEEE754FloatArray((int)reader.ReadBigEndianUint32(), 1);
    }

    void ICurveSegment.Initialize(float minimum, float maximum, float valueAtMinimum)
    {
        Minimum = minimum;
        Maximum = maximum;
        samples[0] = valueAtMinimum;
    }

    /// <inheritdoc />
    public float Evaluate(float input) => 
        Interpolation.InterpolateFloatArray(new ReadOnlySpan<float>(samples), Minimum, Maximum, input);
}