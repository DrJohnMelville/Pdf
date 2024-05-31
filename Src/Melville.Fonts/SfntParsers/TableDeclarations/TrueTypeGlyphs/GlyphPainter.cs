using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.Metrics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

public interface ITrueTypePointTarget
{
    void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd);
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
    public ValueTask RenderGlyphInFontUnits(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix);

}

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
    public  ValueTask RenderGlyphInEmUnits(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix) => 
        RenderGlyphInFontUnits(glyph, target, unitsPerEmCorrection*matrix);

    /// <summary>
    /// Paint a glyph on the indicated target the glyph is expressed in fractions of the 1 unit EM square
    /// </summary>
    /// <param name="glyph">index of the glyph to paint</param>
    /// <param name="target">The target to receive the glyph</param>
    /// <param name="matrix">A transform to apply to the glyph points when rendering.</param>
    public async ValueTask RenderGlyphInFontUnits(uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix)
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