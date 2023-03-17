using System.Buffers;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a type 0 curve segment in a SampledCurveSegment or MultiProcessCurveSet.
/// </summary>
public class FormulaSegmentType0 : ICurveSegment
{
    /// <summary>
    /// Gamma segment parameter
    /// </summary>
    public float Gamma { get; }

    /// <summary>
    /// A segment parameter
    /// </summary>
    public float A { get; }

    /// <summary>
    /// B segment parameter
    /// </summary>
    public float B { get; }

    /// <summary>
    /// C segment parameter
    /// </summary>
    public float C { get; }

    internal FormulaSegmentType0(ref SequenceReader<byte> reader)
    {
        Gamma = reader.ReadIEEE754Float();
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
    }


    /// <inheritdoc />
    public float Evaluate(float input) => (float)(Math.Pow(A * input + B, Gamma) + C);

    void ICurveSegment.Initialize(float minimum, float maximum, float valueAtMinimum)
    {
    }
}

/// <summary>
/// Represents a type 1 curve segment in a SampledCurveSegment or MultiProcessCurveSet.
/// </summary>
public class FormulaSegmentType1 : ICurveSegment
{
    /// <summary>
    /// Gamma segment parameter
    /// </summary>
    public float Gamma { get; }

    /// <summary>
    /// A segment parameter
    /// </summary>
    public float A { get; }

    /// <summary>
    /// B segment parameter
    /// </summary>
    public float B { get; }

    /// <summary>
    /// C segment parameter
    /// </summary>
    public float C { get; }

    /// <summary>
    /// D segment parameter
    /// </summary>
    public float D { get; }

    internal FormulaSegmentType1(ref SequenceReader<byte> reader)
    {
        Gamma = reader.ReadIEEE754Float();
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
        D = reader.ReadIEEE754Float();
    }

    /// <inheritdoc />
    public float Evaluate(float input) =>
        (float)(A * Math.Log10(B * Math.Pow(input, Gamma) + C) + D);

    void ICurveSegment.Initialize(float minimum, float maximum, float valueAtMinimum)
    {
    }
}

/// <summary>
/// Represents a type 2 curve segment in a SampledCurveSegment or MultiProcessCurveSet.
/// </summary>
public class FormulaSegmentType2 : ICurveSegment
{
    /// <summary>
    /// A segment parameter
    /// </summary>
    public float A { get; }

    /// <summary>
    /// B segment parameter
    /// </summary>
    public float B { get; }

    /// <summary>
    /// C segment parameter
    /// </summary>
    public float C { get; }

    /// <summary>
    /// D segment parameter
    /// </summary>
    public float D { get; }

    /// <summary>
    /// E segment parameter
    /// </summary>
    public float E { get; }

    /// <summary>
    /// Parse a formulasegment type 2 from a sequence reader.
    /// </summary>
    /// <param name="reader"></param>
    public FormulaSegmentType2(ref SequenceReader<byte> reader)
    {
        A = reader.ReadIEEE754Float();
        B = reader.ReadIEEE754Float();
        C = reader.ReadIEEE754Float();
        D = reader.ReadIEEE754Float();
        E = reader.ReadIEEE754Float();
    }

    /// <inheritdoc />
    public float Evaluate(float input) => (float)(A * Math.Pow(B, C * input + D) + E);

    void ICurveSegment.Initialize(float minimum, float maximum, float valueAtMinimum)
    {
    }
}