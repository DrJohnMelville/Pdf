namespace Melville.Icc.Model.Tags;

public static class Interpolation
{
    public static float InterpolateFraction(double fraction, float low, float high) => 
        (float)(low + fraction * (high - low));
}