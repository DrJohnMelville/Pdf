using System;
using System.Diagnostics;
using Melville.Hacks;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.SampledFunctions;

internal readonly struct MultiDimensionalArray<T>
{
    private readonly int[] sizes;
    private readonly int[] dimensionLengthInValuesArray;
    private readonly T[] values;

    public MultiDimensionalArray(int[] sizes, int valuesPerIndex, T[] values) : this()
    {
        this.sizes = sizes;
        this.values = values;
        dimensionLengthInValuesArray = CreateCompositeSizes(sizes, valuesPerIndex);
    }
    private static int[] CreateCompositeSizes(int[] inputSizes, int outputCount)
    {
        var ret = new int[inputSizes.Length];
        ret[0] = outputCount;
        for (int i = 1; i < ret.Length; i++)
        {
            ret[i] = ret[i - 1] * inputSizes[i];
        }
        return ret;
    }

    public int ClampToDimension(int value, int dimension) => value.Clamp(0, sizes[dimension]-1);

    public void ReadValues(in ReadOnlySpan<int> index, in Span<T> result)
    {
        Debug.Assert(result.Length == dimensionLengthInValuesArray[0]);
        values.AsSpan(MapMultidimensionalIndex(index), result.Length).CopyTo(result);
    }
    private int MapMultidimensionalIndex(in ReadOnlySpan<int> sampleIndex)
    {
        var baseIndex = 0;
        for (int i = 0; i < sampleIndex.Length; i++)
        {
            baseIndex += dimensionLengthInValuesArray[i] * ClampToDimension(sampleIndex[i], i);
        }

        return baseIndex;
    }
}