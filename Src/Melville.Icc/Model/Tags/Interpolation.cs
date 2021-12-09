namespace Melville.Icc.Model.Tags;

public static class Interpolation
{
    public static float InterpolateFraction(double fraction, float low, float high) => 
        (float)(low + fraction * (high - low));

    public static (T low, T high, float delta) GetInterpolatedPoints<T>(
        in ReadOnlySpan<T> data, float minimum, float maximum, float value)
    {
        var index = (data.Length - 1)*(value - minimum) / (maximum -minimum);
        var bottom = Math.Floor(index);
        var intBottom = (int)bottom;
        if (IsLastPointSpecialCase(data, intBottom)) return (data[intBottom], data[intBottom], 0f);
        var subdelta =(float) (index - bottom);
        return (data[intBottom], data[intBottom+1], subdelta);
   
    }

    private static bool IsLastPointSpecialCase<T>(in ReadOnlySpan<T> data, int intBottom) => 
        intBottom +1 >= data.Length;
}