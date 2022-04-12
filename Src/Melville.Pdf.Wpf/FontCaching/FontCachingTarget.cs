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
    public ValueTask<double> RenderType3Character(Stream s, Matrix3x2 fontMatrix) => 
        throw new NotSupportedException("This should only be used to cache FreeType fonts");
    public IDrawTarget CreateDrawTarget() => this;
   public FillRule Fill() => Geometry?.FillRule ?? FillRule.Nonzero;

    public async ValueTask<CachedGlyph> RenderGlyph(IRealizedFont font, uint glyph)
    {
        var innerender = font.BeginFontWrite(this);
        var width= await innerender.AddGlyphToCurrentString(glyph, Matrix3x2.Identity);
        return new CachedGlyph(Geometry?.GetFlattenedPathGeometry()??new PathGeometry(),
            Fill(), width);
    }
}

public record CachedGlyph(
    PathGeometry Figures, FillRule Rule, double Width)
{
    public PathGeometry CreateInstance(in Transform transform) => 
        new PathGeometry(Figures.Figures, Rule, transform);

    public PathGeometry Original(in Transform transform)
    {
        Figures.Transform = transform;
        return Figures;
    }
}