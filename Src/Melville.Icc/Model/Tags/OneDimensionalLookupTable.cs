namespace Melville.Icc.Model.Tags;

internal static class OneDimensionalLookupTable
{
    public static void LookupInPlace(in Span<float> data, in Span<float> tables)
    {
        if (IsTrivialLookup(tables.Length, data.Length)) return;
        int tableLen = tables.Length / data.Length;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Lookup(data[i], tables.Slice(i * tableLen, tableLen));
        }
    }

    private static bool IsTrivialLookup(int tablesLength, int inputLength) => 
        tablesLength == 0 || inputLength == 0;

    public static float Lookup(float value, in Span<float> points)
    {
        var (low, high, delta) = Interpolation.GetInterpolatedPoints<float>(points, 0, 1, value);
        return Interpolation.InterpolateFraction(delta, low, high);
    }
}