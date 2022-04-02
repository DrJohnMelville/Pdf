using System.Numerics;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;

namespace Melville.Pdf.Wpf.FontCaching;

public class WpfCachedFont : IRealizedFont
{
    private readonly IRealizedFont inner;

    public WpfCachedFont(IRealizedFont inner)
    {
        this.inner = inner;
    }

    public IFontWriteOperation BeginFontWrite(IFontTarget target) => inner.BeginFontWrite(target);
//        new CachedOperation();

    private class CachedOperation : IFontWriteOperation
    {
        public ValueTask<(double width, double height, int charsConsumed)> AddGlyphToCurrentString(
            ReadOnlyMemory<byte> input, Matrix3x2 textMatrix)
        {
            return new((10, 10, 1));
        }

        public void RenderCurrentString(bool stroke, bool fill, bool clip)
        {
        }
    }
}