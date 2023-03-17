using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a transform as a series of defined curves that cover different parts of the domain.
/// </summary>
public class MultiProcessCurve 
{
    /// <summary>
    /// X location of the knots in the multipart curve
    /// </summary>
    public IReadOnlyList<float> BreakPoints { get; }

    private readonly IReadOnlyList<ICurveSegment> segments;
    /// <summary>
    /// Segments of the transformation curve.
    /// </summary>
    public IReadOnlyList<ICurveTag> Segments => segments;

    internal MultiProcessCurve(IReadOnlyList<float> breakPoints, IReadOnlyList<ICurveSegment> segments)
    {
        BreakPoints = breakPoints;
        this.segments = segments;
        InitializeSampledFunctions();
    }

    internal MultiProcessCurve(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32();
        var segmentCount = reader.ReadBigEndianUint16();
        reader.Skip16BitPad();
        BreakPoints = reader.ReadIEEE754FloatArray(segmentCount - 1);
        segments = ReadSegments(ref reader, segmentCount);
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
            segments[i + 1].Initialize(
                BreakPoints[i], BreakPoints[i + 1], Segments[i].Evaluate(BreakPoints[i]));
        }
    }

    /// <summary>
    /// Evaluate a curve transormation
    /// </summary>
    /// <param name="input">the input to the transformation</param>
    /// <returns></returns>
    public float Evaluate(float input)
    {
        int index = 0;
        while (index < BreakPoints.Count && input > BreakPoints[index]) index++;
        return Segments[index].Evaluate(input);
    }
}