using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class IMultiDimensionalLookupTable
{
    
}

public class NullMultiDimensionalLookupTable : IMultiDimensionalLookupTable
{
    public static IMultiDimensionalLookupTable Instance = new NullMultiDimensionalLookupTable();

    private NullMultiDimensionalLookupTable()
    {
    }
}

public class MultidimensionalLookupTable: IMultiDimensionalLookupTable
{
    public IReadOnlyList<int> DimensionLengths { get; }
    public IReadOnlyList<float> Points { get; }

    public MultidimensionalLookupTable(ref SequenceReader<byte> reader, int outputs)
    {
        var dimensions = new int[16];
        DimensionLengths = dimensions;
        ParseDimensionList(ref reader, dimensions);
        var dimensionSize = reader.ReadBigEndianUint8();
        reader.Skip8BitPad();
        reader.Skip16BitPad();
        
        var tableEntries = TotalPoints() * outputs;
        Points = reader.ReadScaledFloatArray(tableEntries, dimensionSize);
        
    }

    private int TotalPoints() => DimensionLengths.Aggregate(1, (agg, i) => agg * Math.Max(1, i));

    private void ParseDimensionList(ref SequenceReader<byte> reader, int[] dimensions)
    {
        for (int i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = reader.ReadBigEndianUint8();
        }
    }
}