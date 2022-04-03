using System.IO;
using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

public class FontCachingTarget : WpfPathCreator, IFontTarget
{
    public ValueTask<(double width, double height)> RenderType3Character(Stream s, Matrix3x2 fontMatrix) => 
        throw new NotSupportedException("This should only be used to cache FreeType fonts");
    public IDrawTarget CreateDrawTarget() => this;
    public PathFigureCollection Figures() => Geometry?.Figures ?? new PathFigureCollection();
    public FillRule Fill() => Geometry?.FillRule ?? FillRule.Nonzero;

    public async ValueTask<CachedGlyph> RenderGlyph(IRealizedFont font, uint glyph)
    {
        var innerender = font.BeginFontWrite(this);
        var (width, height) = await innerender.AddGlyphToCurrentString(glyph, Matrix3x2.Identity);
        Geometry?.Freeze();
        return new CachedGlyph(Figures(), Fill(), width, height);
    }
}

public record CachedGlyph(
    PathFigureCollection Figures, FillRule Rule, double Width, double Height)
{
    public PathGeometry CreateInstance(in Matrix3x2 transform) => 
        new PathGeometry(Figures, Rule, transform.WpfTransform());
}