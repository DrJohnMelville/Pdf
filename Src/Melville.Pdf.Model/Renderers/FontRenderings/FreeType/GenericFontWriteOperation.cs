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

    public async ValueTask AddGlyphToCurrentStringAsync(
        uint character, uint glyph, Matrix3x2 textMatrix)
    {
        await (await parent.GetGlyphSourceAsync().CA())
            .RenderGlyphAsync(glyph, target, textMatrix).CA();
#warning See if we can move the width calculation to only execute when needed.
    }

    public async ValueTask<double> NativeWidthOfLastGlyph(uint glyph) =>
        (await parent.GlyphWidthSourceAsync().CA())
        .GlyphWidth((ushort)glyph);

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