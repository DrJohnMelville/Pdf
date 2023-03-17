using Melville.Icc.Model;

namespace Melville.Icc.ColorTransforms;

/// <summary>
/// Record struct that represents a RGB triple with doubles, valued 0-1
/// </summary>
public record struct DoubleColor(double Red, double Green, double Blue)
{
}

/// <summary>
/// Record struct that represents a RGB tripple with floats.
/// </summary>
public record struct FloatColor(float Red, float Green, float Blue)
{
    /// <summary>
    /// Implicitly convert a DoubleColor to a FloatColor
    /// </summary>
    /// <param name="col">The DoubleColor to convert</param>
    public static implicit operator FloatColor(DoubleColor col) =>
        new((float)col.Red, (float)col.Green, (float)col.Blue);

    /// <summary>
    /// Implicitly convert an XYZNumber to a FloatColor
    /// </summary>
    /// <param name="col"></param>
    public static implicit operator FloatColor(XyzNumber col) =>
        new(col.X, col.Y, col.Z);
}