using System.Buffers;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a color transform specified in the ICC profile as a multidimensional lookup table.
/// </summary>
public class MultidimensionalLookupTable: IColorTransform
{
    /// <summary>
    ///  Number of entries along each dimension of the lookup table
    /// </summary>
    public IReadOnlyList<int> DimensionLengths { get; }

    private readonly float[] points;
    /// <summary>
    /// Lookup table point data
    /// </summary>
    public IReadOnlyList<float> Points => points;

    /// <inheritdoc />
    public int Inputs { get; }

    /// <inheritdoc />
    public int Outputs { get; }

    internal MultidimensionalLookupTable(
        IReadOnlyList<int> dimensionLengths, int outputs, params float[] points)
    {
        DimensionLengths = dimensionLengths;
        this.points = points;
        Outputs = outputs;
        Inputs = DimensionLengths.Count;
    }

    internal MultidimensionalLookupTable(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        Inputs = reader.ReadBigEndianUint16();
        Outputs = reader.ReadBigEndianUint16();
        DimensionLengths = ParseDimensionList(ref reader);
        points = reader.ReadIEEE754FloatArray(TotalPoints());
    }
    
    internal MultidimensionalLookupTable(ref SequenceReader<byte> reader, int outputs)
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

    /// <inheritdoc />
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output)
    {
        Span<int> indices = stackalloc int[Inputs];
        Span<float> scaledInputs = stackalloc float[Inputs];
        ScaleInputsToGrid(input, scaledInputs);
        InnerTransform(scaledInputs, indices, indices.Length -1, output);
    }

    private void ScaleInputsToGrid(ReadOnlySpan<float> input, Span<float> scaledInputs)
    {
        for (int i = 0; i < Inputs; i++)
        {
            // the array contains moth the minimum and maximum point so the actual span of the
            // dimension is one les than the number of points along any dimension
            scaledInputs[i] = Math.Clamp(input[i], 0, 1) * (DimensionLengths[i] - 1);
        }
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

        InterpolateOutputValues(output, fraction, highResult);
    }

    private void Lookup(in Span<int> indices, Span<float> output)
    {
        var size = Outputs;
        var position = 0;
        for (int i = indices.Length -1; i >= 0; i--)
        {
            position += size * indices[i];
            size *= DimensionLengths[i];
        }
        points.AsSpan().Slice(position, Outputs).CopyTo(output);
    }

    private bool IsMaximumSpecialCase(Span<int> indices, int activeIndex) =>
        indices[activeIndex] >= DimensionLengths[activeIndex];

    private static void InterpolateOutputValues(Span<float> output, float fraction, Span<float> highResult)
    {
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = Interpolation.InterpolateFraction(fraction, output[i], highResult[i]);
        }
    }
}