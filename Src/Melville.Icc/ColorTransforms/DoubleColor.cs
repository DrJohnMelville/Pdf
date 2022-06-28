using Melville.Icc.Model;

namespace Melville.Icc.ColorTransforms;

public record struct DoubleColor(double Red, double Green, double Blue)
{
}

public record struct FloatColor(float Red, float Green, float Blue)
{
    public static implicit operator FloatColor(DoubleColor col) =>
        new((float)col.Red, (float)col.Green, (float)col.Blue);
    public static implicit operator FloatColor(XyzNumber col) =>
        new(col.X, col.Y, col.Z);
}