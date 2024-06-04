using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

/// <summary>
/// Receives the points that make up a TrueType Glyph outline.
/// </summary>
public interface ITrueTypePointTarget
{
    /// <summary>
    /// Add a real point to the glyph
    /// </summary>
    /// <param name="point">Location of the point</param>
    /// <param name="onCurve">True if this is a curve point, false if it is a control point</param>
    /// <param name="isContourStart">True if this is the first point in a contour, false otherwise.</param>
    /// <param name="isContourEnd">True if this is the last point in a contour, false otherwise</param>
    void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd);
    /// <summary>
    /// Add a phantom point, which does not render but can affect the shape of composite glyphs.
    /// </summary>
    /// <param name="point"></param>
    void AddPhantomPoint(Vector2 point);
}

internal interface ISubGlyphRenderer
{
    /// <summary>
    /// Paint a glyph on the indicated target the glyph is expressed in the
    /// native units in which the font is defined.  This is useful for rendering subglyphs.
    /// </summary>
    /// <param name="glyph">index of the glyph to paint</param>
    /// <param name="target">The target to receive the glyph</param>
    /// <param name="matrix">A transform to apply to the glyph points when rendering.</param>
    public ValueTask RenderGlyphInFontUnitsAsync(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix);

}

/// <summary>
/// This class rertieves glyphs from a SFnt font that uses truetype outlines.
/// </summary>
public class TrueTypeGlyphSource: IGlyphSource, ISubGlyphRenderer
{
    private readonly IGlyphLocationSource index;
    private readonly IMultiplexSource glyphDataOrigin;
    private readonly Matrix3x2 unitsPerEmCorrection;
    private readonly ParsedHorizontalMetrics hMetrics;

    internal TrueTypeGlyphSource(
        IGlyphLocationSource index, 
        IMultiplexSource glyphDataOrigin, 
        uint unitsPerEm,
        ParsedHorizontalMetrics hMetrics)
    {
        this.index = index;
        this.glyphDataOrigin = glyphDataOrigin;
        this.hMetrics = hMetrics;
        unitsPerEmCorrection = Matrix3x2.CreateScale(1.0f / unitsPerEm);
    }

    /// <inheritdoc />
    public int GlyphCount => index.TotalGlyphs;

    /// <summary>
    /// Paint a glyph on the indicated target the glyph is expressed in fractions of the 1 unit EM square
    /// </summary>
    /// <param name="glyph">index of the glyph to paint</param>
    /// <param name="target">The target to receive the glyph</param>
    /// <param name="matrix">A transform to apply to the glyph points when rendering.</param>
    public  ValueTask RenderGlyphInEmUnitsAsync(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix) => 
        RenderGlyphInFontUnitsAsync(glyph, target, unitsPerEmCorrection*matrix);

    /// <summary>
    /// Paint a glyph on the indicated target the glyph is expressed in fractions of the 1 unit EM square
    /// </summary>
    /// <param name="glyph">index of the glyph to paint</param>
    /// <param name="target">The target to receive the glyph</param>
    /// <param name="matrix">A transform to apply to the glyph points when rendering.</param>
    public async ValueTask RenderGlyphInFontUnitsAsync(uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix)
    {
        var location = index.GetLocation(glyph);
        if (location.Length == 0) return;
        var data = await glyphDataOrigin.ReadPipeFrom(location.Offset)
            .ReadAtLeastAsync((int)location.Length).CA();
        await new TrueTypeGlyphParser(
                this, data.Buffer.Slice(0, location.Length), target, 
                matrix, hMetrics[(int)glyph])
            .DrawGlyphAsync().CA();
    }
}