using System.Numerics;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal class GenericFontWriteOperation(IGenericFont parent, IDrawTarget target) : IFontWriteOperation
{
    public virtual void Dispose()
    {
    }

    public async ValueTask<double> AddGlyphToCurrentStringAsync(
        uint character, uint glyph, Matrix3x2 textMatrix)
    {
//        target.SetDrawingTransform(textMatrix);
        await (await parent.GetGlyphSourceAsync().CA())
            .RenderGlyphAsync(glyph, target, textMatrix).CA();
        return (await parent.GlyphWidthSourceAsync().CA())
            .GlyphWidth((ushort)glyph);
#warning See if we can move the width calculation to only execute when needed.
    }

    public void RenderCurrentString(bool stroke, bool fill, bool clip, in Matrix3x2 textMatrix)
    {
        if (stroke || fill)
        {
            target.PaintPath(stroke, fill, GlyphRequiresEvenOddFill());
        }

        if (clip)
        {
            target.ClipToPath(GlyphRequiresEvenOddFill());
        }
    }

    private bool GlyphRequiresEvenOddFill()
    {
        return false;
    }

    public IFontWriteOperation CreatePeerWriteOperation(IFontTarget target) =>
        new GenericFontWriteOperation(parent, target.CreateDrawTarget());
}