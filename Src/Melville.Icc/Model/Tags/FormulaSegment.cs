using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

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

    public float Evaluate(float input) => (float)(Math.Pow(A * input + B, Gamma) + C);
    public void Initialize(float minimum, float maximum, float valueAtMinimum)
    { 
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

    public float Evaluate(float input) =>
        (float)(A * Math.Log10(B * Math.Pow(input, Gamma) + C) + D);

    public void Initialize(float minimum, float maximum, float valueAtMinimum)
    { 
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

    public float Evaluate(float input) => (float)(A * Math.Pow(B, C * input + D) + E);
    public void Initialize(float minimum, float maximum, float valueAtMinimum)
    { 
    }
}