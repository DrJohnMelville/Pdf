using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

internal interface ISubGlyphRenderer
{
    /// <summary>
    /// Paint a glyph on the indicated target the glyph is expressed in the
    /// native units in which the font is defined.  This is useful for rendering subglyphs.
    /// </summary>
    /// <param name="glyph">index of the glyph to paint</param>
    /// <param name="target">The target to receive the glyph</param>
    /// <param name="matrix">A transform to apply to the glyph points when rendering.</param>
    public ValueTask RenderGlyphInFontUnitsAsync<T>(
        uint glyph, T target, Matrix3x2 matrix) where T:ITrueTypePointTarget;

}