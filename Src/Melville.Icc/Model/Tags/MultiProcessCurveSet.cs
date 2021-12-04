using System.Buffers;
using System.ComponentModel.DataAnnotations.Schema;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public interface ICurveSegment
{
}

public class MultiProcessCurve : ProfileData
{
    public IReadOnlyList<float> BreakPoints { get; } 
    public IReadOnlyList<ICurveSegment> Segments { get; }
    public MultiProcessCurve(ref SequenceReader<byte> reader)
    {
        reader.ReadBigEndianUint32();
        var segments = new ICurveSegment[reader.ReadBigEndianUint16()];
        reader.Skip16BitPad();
        BreakPoints = reader.ReadIEEE754FloatArray(segments.Length - 1);
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i] = (ICurveSegment)TagParser.Parse(ref reader);
        }
        Segments = segments;
    }
}

public class MultiProcessCurveSet : ProfileData, IMultiProcessElement
{
    public int Inputs { get; }
    public int Outputs => Inputs;
    public IReadOnlyList<MultiProcessCurve> Curves { get; }
    public MultiProcessCurveSet(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint16();
        VerifyCorrectOutputNumber(reader.ReadBigEndianUint16());
        var curves = new MultiProcessCurve[Inputs];
        for (int i = 0; i < curves.Length; i++)
        {
            var innerReader = reader.ReadPositionNumber();
            curves[i] = (MultiProcessCurve)TagParser.Parse(ref innerReader);
        }

        Curves = curves;
    }

    private void VerifyCorrectOutputNumber(ushort outputs)
    {
        if (outputs != Inputs)
            throw new InvalidDataException("Curve set must have same number of inputs and outputs");
    }
}