using System.IO;
using System.Numerics;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Wpf.Rendering;

namespace Melville.Pdf.Wpf.FontCaching;

public class WpfCachedFont : IRealizedFont
{
    private readonly IRealizedFont inner;

    public WpfCachedFont(IRealizedFont inner)
    {
        this.inner = inner;
    }

    public (uint glyph, int charsConsumed) GetNextGlyph(in ReadOnlySpan<byte> input) =>
        inner.GetNextGlyph(input);

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => 
       inner.BeginFontWrite(target);
       // new CachedOperation(RequiredDrawTargetToTargetWpf(target.CreateDrawTarget()));

    private static WpfDrawTarget RequiredDrawTargetToTargetWpf(IDrawTarget target) => 
        (WpfDrawTarget)target;

    private class CachedOperation : IFontWriteOperation
    {
        private readonly WpfDrawTarget innerTarget;

        public CachedOperation(WpfDrawTarget innerTarget)
        {
            this.innerTarget = innerTarget;
        }

        public ValueTask<(double width, double height)> AddGlyphToCurrentString(uint glyph, Matrix3x2 textMatrix)
        {
            return new((10.0, 10.0));
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
        }
    }
}