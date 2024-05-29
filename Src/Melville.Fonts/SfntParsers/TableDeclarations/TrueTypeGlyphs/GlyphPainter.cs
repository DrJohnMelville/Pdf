using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

public interface ITrueTypePointTarget
{
    void AddPoint(Vector2 point, bool onCurve, bool isContourStart, bool isContourEnd);
    void AddPhantomPoint(Vector2 point);
}

public class TrueTypeGlyphSource: IGlyphSource
{
    private readonly IGlyphLocationSource index;
    private readonly IMultiplexSource glyphDataOrigin;
    private readonly Matrix3x2 unitsPerEmCorrection;

    public TrueTypeGlyphSource(IGlyphLocationSource index, IMultiplexSource glyphDataOrigin, uint unitsPerEm)
    {
        this.index = index;
        this.glyphDataOrigin = glyphDataOrigin;
        unitsPerEmCorrection = Matrix3x2.CreateScale(1.0f / unitsPerEm);
    }

    public int GlyphCount => index.TotalGlyphs;

    public async ValueTask ParsePointsAsync(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix)
    {
        var location = index.GetLocation(glyph);
        if (location.Length == 0) return;
        var data = await glyphDataOrigin.ReadPipeFrom(location.Offset)
            .ReadAtLeastAsync((int)location.Length).CA();
        await new TrueTypeGlyphParser(
                this, data.Buffer.Slice(0, location.Length), target, 
                unitsPerEmCorrection*matrix, 0)
            .DrawGlyphAsync().CA();
    }
}