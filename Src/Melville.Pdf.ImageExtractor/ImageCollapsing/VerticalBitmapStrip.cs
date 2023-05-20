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

    protected override async ValueTask RenderPbgra(PointerHolder buffer)
    {
        long offset = componentBitmaps.Sum(i=>i.ReqiredBufferSize());
        foreach (var singleBitmap in componentBitmaps)
        {
            offset -= singleBitmap.ReqiredBufferSize();
            await buffer.WriteToStream(offset, singleBitmap);
        }
    }
}