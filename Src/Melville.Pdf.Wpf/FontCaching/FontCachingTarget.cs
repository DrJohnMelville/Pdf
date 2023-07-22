using System.IO;
using System.Numerics;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

internal class FontCachingTarget : WpfPathCreator, IFontTarget
{
    public ValueTask<double> RenderType3CharacterAsync(Stream s, Matrix3x2 fontMatrix,
        PdfValueDictionary fontDictionary) => 
        throw new NotSupportedException("This should only be used to cache FreeType fonts");
    public IDrawTarget CreateDrawTarget() => this;
   public FillRule Fill() => Geometry?.FillRule ?? FillRule.Nonzero;

    public async ValueTask<CachedGlyph> RenderGlyphAsync(
        IFontWriteOperation innerender, uint character, uint glyph)
    {
        var width= await innerender.AddGlyphToCurrentStringAsync(character, glyph, Matrix3x2.Identity);
        var finalGeometry = Geometry??new PathGeometry();
        return new CachedGlyph(finalGeometry, Fill(), width);
    }
}

internal record CachedGlyph(
    PathGeometry Figures, FillRule Rule, double Width)
{
    public PathGeometry CreateInstance(in Transform transform) => new(Figures.Figures, Rule, transform);

    public PathGeometry Original(in Transform transform)
    {
        Figures.Transform = transform;
        return Figures;
    }
}