namespace Melville.Icc.Model.Tags;

public static class OneDimensionalLookupTable
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
        var position = value / (points.Length - 1);
        var min = (int)(position);
        return MaxValueSpecialCase(points, min) ? 
            points[min] : 
            Interpolation.InterpolateFraction(position - min, points[min], points[min + 1]);
    }

    private static bool MaxValueSpecialCase(in Span<float> points, int min) => 
        min + 1 >= points.Length;
}