using System.IO;
using System.Numerics;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

internal class FontCachingTarget : WpfPathCreator, IFontTarget
{
    public ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
        PdfDictionary fontDictionary) =>
        throw new NotSupportedException("This should only be used to cache FreeType fonts");

    public IRenderTarget RenderTarget => 
        throw new NotSupportedException("This should only be used to cache FreeType fonts");

    public IDrawTarget CreateDrawTarget() => this;
    public FillRule Fill() => Geometry?.FillRule ?? FillRule.Nonzero;

    public async ValueTask<CachedGlyph> RenderGlyphAsync(
        IFontWriteOperation innerender, uint character, uint glyph)
    {
        await innerender.AddGlyphToCurrentStringAsync(character, glyph, Matrix3x2.Identity);
        var finalGeometry = Geometry ?? new PathGeometry();
        return new CachedGlyph(finalGeometry, Fill());
    }
}

internal record CachedGlyph(
    PathGeometry Figures, FillRule Rule)
{
    private double? width;

    public PathGeometry CreateInstance(in Transform transform) => 
        new(Figures.Figures, Rule, transform);

    public PathGeometry Original(in Transform transform)
    {
        Figures.Transform = transform;
        return Figures;
    }

    public ValueTask<double> ComputeWidthAsync(IFontWriteOperation innerWriter, uint glyph)
    {
        return width.HasValue?new(width.Value):InnerCreateWidthAsync(innerWriter, glyph);
    }

    private async ValueTask<double> InnerCreateWidthAsync(IFontWriteOperation innerWriter, uint glyph)
    {
        return (width = await innerWriter.NativeWidthOfLastGlyphAsync(glyph))??0;
    }
}