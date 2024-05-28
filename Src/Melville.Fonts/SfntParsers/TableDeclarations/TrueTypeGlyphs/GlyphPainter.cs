using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

public interface ITrueTypePointTarget
{
    void AddPoint(double x, double y, bool onCurve, bool isContourStart, bool isContourEnd);
}

public partial class TrueTypeGlyphSource: IGlyphSource
{
    [FromConstructor] private readonly IGlyphLocationSource index;
    [FromConstructor] private readonly IMultiplexSource glyphDataOrigin;

    public int GlyphCount => index.TotalGlyphs;

    public async ValueTask ParsePointsAsync(
        uint glyph, ITrueTypePointTarget target, Matrix3x2 matrix)
    {
        var location = index.GetLocation(glyph);
        if (location.Length == 0) return;
        var data = await glyphDataOrigin.ReadPipeFrom(location.Offset)
            .ReadAtLeastAsync((int)location.Length).CA();
        await new TrueTypeGlyphParser(this, data.Buffer.Slice(0, location.Length), target, matrix)
            .DrawGlyphAsync().CA();
    }
}