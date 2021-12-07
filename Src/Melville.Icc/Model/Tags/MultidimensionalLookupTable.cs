using System.Buffers;
using Melville.Icc.Parser;
using SequenceReaderExtensions = System.Buffers.SequenceReaderExtensions;

namespace Melville.Icc.Model.Tags;

public class MultidimensionalLookupTable: IColorTransform
{
    public IReadOnlyList<int> DimensionLengths { get; }

    private readonly float[] points;
    public IReadOnlyList<float> Points => points;

    public int Inputs { get; }
    public int Outputs { get; }

    public MultidimensionalLookupTable(
        IReadOnlyList<int> dimensionLengths, int outputs, params float[] points)
    {
        DimensionLengths = dimensionLengths;
        this.points = points;
        Outputs = outputs;
    }

    public MultidimensionalLookupTable(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint16();
        Outputs = reader.ReadBigEndianUint16();
        DimensionLengths = ParseDimensionList(ref reader);
        points = reader.ReadIEEE754FloatArray(TotalPoints());
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
        points = reader.ReadScaledFloatArray(tableEntries, dimensionSize);
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
        Span<int> indices = stackalloc int[input.Length];
        Span<float> scaledInputs = stackalloc float[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            scaledInputs[i] = Math.Clamp(input[i], 0, 1) * (DimensionLengths[i]-1);
        }
        InnerTransform(scaledInputs, indices, indices.Length -1, output);
    }

    private void InnerTransform(
        in Span<float> inputs, in Span<int> indices, int activeIndex, in Span<float> output)
    {
        if (activeIndex < 0)
        {
            Lookup(indices, output);
            return;
        }
        indices[activeIndex] = (int)inputs[activeIndex];
        var fraction = inputs[activeIndex] - indices[activeIndex];
        InnerTransform(inputs, indices, activeIndex-1, output);
        indices[activeIndex]++;
        if (IsMaximumSpecialCase(indices, activeIndex)) return;
        Span<float> highResult = stackalloc float[output.Length];
        InnerTransform(inputs, indices, activeIndex-1, highResult);

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = Interpolation.InterpolateFraction(fraction, output[i], highResult[i]);
        }
    }
    private bool IsMaximumSpecialCase(Span<int> indices, int activeIndex) =>
        indices[activeIndex] >= DimensionLengths[activeIndex];

    public void Lookup(in Span<int> indices, Span<float> output)
    {
        var size = Outputs;
        int position = 0;
        for (int i = indices.Length -1; i >= 0; i--)
        {
            position += size * indices[i];
            size *= DimensionLengths[i];
        }
        points.AsSpan().Slice(position, Outputs).CopyTo(output);
    }
}