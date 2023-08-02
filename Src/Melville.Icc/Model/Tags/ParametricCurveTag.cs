using System.Buffers;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a scalar function.
/// </summary>
public interface ICurveTag
{
    /// <summary>
    /// Evaluate the curve function for a given input.
    /// </summary>
    /// <param name="input">The input scalar</param>
    /// <returns>Scalar output of the function.</returns>
    public float Evaluate(float input);
}

/// <summary>
/// A null object pattern for ICurveTag -- a curve that always returns its input
/// </summary>
[StaticSingleton]
public partial class NullCurve : ICurveTag
{
    /// <inheritdoc />
    public float Evaluate(float input) => input;
}

/// <summary>
/// Evaluates one of a number of paarametric curves from table 65 in the spec
/// </summary>
public partial class ParametricCurveTag : ICurveTag
{
    /// <summary>
    /// Curve parameter G
    /// </summary>
    public float G { get; }

    /// <summary>
    /// Curve parameter A
    /// </summary>
    public float A { get; }

    /// <summary>
    /// Curve parameter B
    /// </summary>
    public float B { get; }

    /// <summary>
    /// Curve parameter C
    /// </summary>
    public float C { get; }

    /// <summary>
    /// Curve parameter D
    /// </summary>
    public float D { get; }

    /// <summary>
    /// Curve parameter E
    /// </summary>
    public float E { get; }

    /// <summary>
    /// Curve parameter F
    /// </summary>
    public float F { get; }

    /// <inheritdoc />
    public float Evaluate(float input)
    {
        return input >= D ? (float)(Math.Pow(A * input + B, G) + C) : E * input + F;
    }

    internal ParametricCurveTag(float g, float a, float b, float c, float d, float e, float f)
    {
        G = g;
        A = a;
        B = b;
        C = c;
        D = d;
        E = e;
        F = f;
    }

    internal ParametricCurveTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var type = reader.ReadBigEndianUint16();
        reader.Skip16BitPad();
        (G, A, B, C, D, E, F) = ReadParamsForType(ref reader, type);
    }

    private (float G, float A, float B, float C, float D, float E, float F) ReadParamsForType(
        ref SequenceReader<byte> reader, ushort type) => type switch
    {
        0 => ReadType0Function(ref reader),
        1 => ReadType1Function(ref reader),
        2 => ReadType2Function(ref reader),
        3 => ReadType3Function(ref reader),
        4 => ReadType4Function(ref reader),
        _ => throw new InvalidDataException("Unknown parametric function type")
    };

    private static (float, float, float, float, float MinValue, float, float) ReadType0Function(
        ref SequenceReader<byte> reader) => (reader.Reads15Fixed16(), 1f, 0f, 0f, float.MinValue, 0f, 0f);

    private static (float G, float A, float B, float C, float D, float E, float F)
        ReadType1Function(ref SequenceReader<byte> reader)
    {
        var g = reader.Reads15Fixed16();
        var a = reader.Reads15Fixed16();
        var b = reader.Reads15Fixed16();
        return (g, a, b, 0f, -b / a, 0f, 0f);
    }

    private static (float G, float A, float B, float C, float D, float E, float F)
        ReadType2Function(ref SequenceReader<byte> reader) =>
        ReadType1Function(ref reader) with { C = reader.Reads15Fixed16() };

    private (float G, float A, float B, float C, float D, float E, float F) ReadType3Function(
        ref SequenceReader<byte> reader)
    {
        // separate method because we need to read the arguments out of order
        var g = reader.Reads15Fixed16();
        var a = reader.Reads15Fixed16();
        var b = reader.Reads15Fixed16();
        var e = reader.Reads15Fixed16();
        var d = reader.Reads15Fixed16();
        return (g, a, b, 0, d, e, 0);
    }

    private (float G, float A, float B, float C, float D, float E, float F) ReadType4Function(
        ref SequenceReader<byte> reader) =>
        (reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16(),
            reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16(), reader.Reads15Fixed16());
}