namespace Melville.Icc.Model.Tags;

public static class OneDimensionalLookupTable
{
    public static void MultiLookup(in Span<float> intermed, in Span<float> tables)
    {
        int tableLen = tables.Length / intermed.Length;
        for (int i = 0; i < intermed.Length; i++)
        {
            intermed[i] = Lookup(intermed[i], tables.Slice(i * tableLen, tableLen));
        }
    }
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