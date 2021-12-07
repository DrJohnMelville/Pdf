using System.Buffers;
using Melville.Icc.Parser;
using SequenceReaderExtensions = System.Buffers.SequenceReaderExtensions;

namespace Melville.Icc.Model.Tags;

public class NullColorTransform : IColorTransform
{
    public static NullColorTransform Instance(int inputs) => inputs switch {
        1 => one,
        2 => two,
        3 => three,
        4 => four,
        5 => five,
        _ => new NullColorTransform(inputs)
    };

    public int Inputs { get; }
    public int Outputs => Inputs;

    private NullColorTransform(int inputs)
    {
        Inputs = inputs;
    }
    
    private static readonly NullColorTransform one = new(1);
    private static readonly NullColorTransform two = new(2);
    private static readonly NullColorTransform three = new(3);
    private static readonly NullColorTransform four = new(4);
    private static readonly NullColorTransform five = new(5);

    public static NullColorTransform Parse(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        return Instance(reader.ReadBigEndianUint16());
    }
    
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        throw new NotImplementedException();
    }
}

public class MultidimensionalLookupTable: IColorTransform
{
    public IReadOnlyList<int> DimensionLengths { get; }
    public IReadOnlyList<float> Points { get; }

    public int Inputs { get; }
    public int Outputs { get; }
    public MultidimensionalLookupTable(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint16();
        Outputs = reader.ReadBigEndianUint16();
        DimensionLengths = ParseDimensionList(ref reader);
        Points = reader.ReadIEEE754FloatArray(TotalPoints());
    }
    
    public MultidimensionalLookupTable(ref SequenceReader<byte> reader, int outputs)
    { 
        DimensionLengths = ParseDimensionList(ref reader);
        var dimensionSize = reader.ReadBigEndianUint8();
        reader.Skip8BitPad();
        reader.Skip16BitPad();
        Outputs = outputs;
        Inputs = DimensionLengths.Count(i => i > 0);
        var tableEntries = TotalPoints();
        Points = reader.ReadScaledFloatArray(tableEntries, dimensionSize);
    }

    private int TotalPoints() => DimensionLengths.Aggregate(Outputs, (agg, i) => agg * Math.Max(1, i));

    private int[] ParseDimensionList(ref SequenceReader<byte> reader)
    {
        var dimensions = new int[16];
        for (int i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = reader.ReadBigEndianUint8();
        }
        return dimensions;
    }
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        throw new NotImplementedException();
    }
}