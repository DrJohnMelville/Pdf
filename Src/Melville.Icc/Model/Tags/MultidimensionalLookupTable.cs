using System.Buffers;
using Melville.Icc.Parser;
using SequenceReaderExtensions = System.Buffers.SequenceReaderExtensions;

namespace Melville.Icc.Model.Tags;

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

    private void InnerTransform(
        in ReadOnlySpan<float> inputs, in Span<int> indices, int activeIndex, in Span<float> output)
    {
        
    }
}