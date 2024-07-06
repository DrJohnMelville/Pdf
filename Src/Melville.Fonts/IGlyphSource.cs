using System.Numerics;

namespace Melville.Fonts;

/// <summary>
/// This interface is used to retrieve individual glyphs from the font file  
/// </summary>
public interface IGlyphSource
{
    /// <summary>
    /// The number of glyphs in the font.
    /// </summary>
    int GlyphCount { get; }

    /// <summary>
    /// Render the selected Glyph to the given target 
    /// </summary>
    /// <typeparam name="T">The type of glyph target to render to.</typeparam>
    /// <param name="glyph">The glyph to render to.</param>
    /// <param name="target">The target of the write operation</param>
    /// <param name="transform">A transform to convert a unit square to the desired glyph size,
    /// location, and orientation</param>
    ValueTask RenderGlyphAsync<T>(uint glyph, T target, Matrix3x2 transform) 
        where T : IGlyphTarget;
}