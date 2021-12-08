using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public interface ICurveSegment
{
    float Evaluate(float input);
    void Initialize(float minimum, float maximum, float valueAtMinimum);
}

public class MultiProcessCurve 
{
    public IReadOnlyList<float> BreakPoints { get; } 
    public IReadOnlyList<ICurveSegment> Segments { get; }

    public MultiProcessCurve(IReadOnlyList<float> breakPoints, IReadOnlyList<ICurveSegment> segments)
    {
        BreakPoints = breakPoints;
        Segments = segments;
        InitializeSampledFunctions();
    }

    public MultiProcessCurve(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32();
        var segmentCount = reader.ReadBigEndianUint16();
        reader.Skip16BitPad();
        BreakPoints = reader.ReadIEEE754FloatArray(segmentCount - 1);
        Segments = ReadSegments(ref reader, segmentCount);
        InitializeSampledFunctions();
    }

    private static ICurveSegment[] ReadSegments(ref SequenceReader<byte> reader, int count)
    {
        var segments = new ICurveSegment[count];
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i] = (ICurveSegment)TagParser.Parse(ref reader);
        }
        return segments;
    }

    private void InitializeSampledFunctions()
    {
        for (int i = 0; i < BreakPoints.Count - 1; i++)
        {
            Segments[i + 1].Initialize(
                BreakPoints[i], BreakPoints[i + 1], Segments[i].Evaluate(BreakPoints[i]));
        }
    }

    public float Evaluate(float input)
    {
        int index = 0;
        while (index < BreakPoints.Count && input > BreakPoints[index]) index++;
        return Segments[index].Evaluate(input);
    }
}

public class MultiProcessCurveSet : IColorTransform
{
    public int Inputs => Curves.Count;
    public int Outputs => Inputs;
    public IReadOnlyList<MultiProcessCurve> Curves { get; }

    public MultiProcessCurveSet(params MultiProcessCurve[] curves)
    {
        Curves = curves;
    }

    public MultiProcessCurveSet(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var curves = new MultiProcessCurve[reader.ReadBigEndianUint16()];
        Curves = curves;
        VerifyCorrectOutputNumber(reader.ReadBigEndianUint16());
        for (int i = 0; i < curves.Length; i++)
        {
            var innerReader = reader.ReadPositionNumber();
            curves[i] = (MultiProcessCurve)TagParser.Parse(ref innerReader);
        }

    }

    private void VerifyCorrectOutputNumber(ushort outputs)
    {
        if (outputs != Inputs)
            throw new InvalidDataException("Curve set must have same number of inputs and outputs");
    }
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        for (int i = 0; i < Inputs; i++)
        {
            output[i] = Curves[i].Evaluate(input[i]);
        }
    }

}