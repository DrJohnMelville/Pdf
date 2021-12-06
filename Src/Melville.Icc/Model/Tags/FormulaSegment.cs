using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class FormulaSegmentType0 : ICurveSegment
{
    public float Gamma { get; }
    public float A { get; }
    public float B { get; }
    public float C { get; }
    public FormulaSegmentType0(ref SequenceReader<byte> reader)
    {
        Gamma = reader.ReadIEEE754Float();
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
    }
}

public class FormulaSegmentType1 : ICurveSegment
{
    public float Gamma { get; }
    public float A { get; }
    public float B { get; }
    public float C { get; }
    public float D { get; }
    public FormulaSegmentType1(ref SequenceReader<byte> reader)
    {
        Gamma = reader.ReadIEEE754Float();
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
        D = reader.ReadIEEE754Float();
    }
}

public class FormulaSegmentType2 : ICurveSegment
{
    public float A { get; }
    public float B { get; }
    public float C { get; }
    public float D { get; }
    public float E { get; }

    public FormulaSegmentType2(ref SequenceReader<byte> reader)
    {
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
        D = reader.ReadIEEE754Float();
        E = reader.ReadIEEE754Float();
    }
}

public class SampledCurveSegment : ICurveSegment
{
    public IReadOnlyList<float> Samples { get; }
    public SampledCurveSegment(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Samples = reader.ReadIEEE754FloatArray((int)reader.ReadBigEndianUint32());
    }
}