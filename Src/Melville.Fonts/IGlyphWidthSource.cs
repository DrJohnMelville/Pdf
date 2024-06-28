using System.Numerics;

namespace Melville.Fonts;

public interface IGlyphWidthSource
{
    /// <summary>
    /// Compute the glyph width for a given glyph in fractions of te EM width.
    /// </summary>
    /// <param name="glyph">The required glyph</param>
    /// <returns>The width of the glyph in fractional EM units.</returns>
    float GlyphWidth(ushort glyph);
}