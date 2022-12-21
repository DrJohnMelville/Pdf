namespace Melville.Icc.Model;

/// <summary>
/// Describes the desired attributes of a color conversion
/// </summary>
public enum RenderIntent : uint
{
    /// <summary>
    /// Preserve the perceptual qualities of complete images containing colors out of gamut in the target colorspace/
    /// </summary>
    Perceptual = 0,
    /// <summary>
    /// Maps the white point of the source to the white point of the destination.  Out of gamut colors are replaced with the nearest color.
    /// </summary>
    RelativeColorimetric = 1,
    /// <summary>
    /// Preserve saturation at the expense of color accuracy.
    /// </summary>
    Saturation = 2,
    /// <summary>
    /// Preserve original color as much as possible.  Out of gamut colors are converted to nearest color.
    /// </summary>
    AbsoluteColorimetric = 3,
}