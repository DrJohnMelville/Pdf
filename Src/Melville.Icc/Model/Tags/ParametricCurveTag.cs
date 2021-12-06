using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public interface ICurveTag{}

public class NullCurve : ICurveTag
{
    public static ICurveTag Instance = new NullCurve();
    private NullCurve(){}
}

public class ParametricCurveTag: ICurveTag
{
    public float G { get; }
    public float A { get; }
    public float B { get; }
    public float C { get; }
    public float D { get; }
    public float E { get; }
    public float F { get; }
    public ParametricCurveTag(ref SequenceReader<byte> reader)
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
        ref SequenceReader<byte> reader) => (reader.Reads15Fixed16(), 1f,0f, 0f, float.MinValue, 0f, 0f);

    private static (float G, float A, float B, float C, float D, float E, float F) 
        ReadType1Function(ref SequenceReader<byte> reader)
    {
        var g = reader.Reads15Fixed16();
        var a = reader.Reads15Fixed16();
        var b = reader.Reads15Fixed16();
        return (g, a,b, 0f, -b/a, 0f, 0f);
    }

    private static (float G, float A, float B, float C, float D, float E, float F) 
        ReadType2Function(ref SequenceReader<byte> reader) => 
        ReadType1Function(ref reader) with {C = reader.Reads15Fixed16()};

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