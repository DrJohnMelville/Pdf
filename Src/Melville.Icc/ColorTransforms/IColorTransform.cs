namespace Melville.Icc.ColorTransforms;

/// <summary>
/// Implements a color transform, typically one defined by an ICC profile.
/// </summary>
public interface IColorTransform
{
    /// <summary>
    /// Number of parameters in the input color space.
    /// </summary>
    public int Inputs { get; }
    /// <summary>
    /// Number of parameters in the output color space.
    /// </summary>
    public int Outputs { get; }
    /// <summary>
    /// Compute the color transform.
    /// </summary>
    /// <param name="input">A span of Inputs floats representing the input color</param>
    /// <param name="output">A span of Outputs floats that the output color will be written to.  If Inputs equals
    /// outputs then input and output may be the same span.</param>
    public void Transform(in ReadOnlySpan<float> input, in Span<float> output);
}