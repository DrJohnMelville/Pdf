using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing;

public partial class VerticalBitmapStrip : BitmapStrip
{
    public override int Width => FirstChild.Width;
    public override int Height => componentBitmaps.Sum(i => i.Height);
    public override Vector2 PositionBottomLeft => LastChild.PositionBottomLeft;
    public override Vector2 PositionBottomRight => LastChild.PositionBottomRight;
    public override Vector2 PositionTopLeft => FirstChild.PositionTopLeft;
    public override Vector2 PositionTopRight => FirstChild.PositionTopRight;

    protected override async ValueTask RenderPbgraAsync(PointerHolder buffer)
    {
        long offset = 0;
        foreach (var singleBitmap in componentBitmaps)
        {
            await buffer.WriteToStreamAsync(offset, singleBitmap);
            offset += singleBitmap.ReqiredBufferSize();
        }
    }
}