using System.Numerics;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing;

internal partial class VerticalBitmapStrip : BitmapStrip
{
    public override int Width => FirstChild.Width;
    public override int Height => componentBitmaps.Sum(i => i.Height);
    public override Vector2 PositionBottomLeft => LastChild.PositionBottomLeft;
    public override Vector2 PositionBottomRight => LastChild.PositionBottomRight;
    public override Vector2 PositionTopLeft => FirstChild.PositionTopLeft;
    public override Vector2 PositionTopRight => FirstChild.PositionTopRight;

    public override async ValueTask RenderPbgraAsync(Memory<DeviceColor> buffer)
    {
        var next = buffer;
        foreach (var singleBitmap in componentBitmaps)
        {
            await singleBitmap.RenderPbgraAsync(next);
            next = next[singleBitmap.TotalPixels()..];
        }
    }
}