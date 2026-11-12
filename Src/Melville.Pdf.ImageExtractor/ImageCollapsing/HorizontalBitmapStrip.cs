using System.Diagnostics.Contracts;
using System.Numerics;
using Melville.INPC;

using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing;

internal partial class HorizontalBitmapStrip : BitmapStrip
{
    public override int Width => componentBitmaps.Sum(i => i.Width);
    public override int Height => FirstChild.Height;
    public override Vector2 PositionBottomLeft => FirstChild.PositionBottomLeft;
    public override Vector2 PositionBottomRight => LastChild.PositionBottomRight;
    public override Vector2 PositionTopLeft => FirstChild.PositionTopLeft;
    public override Vector2 PositionTopRight => LastChild.PositionTopRight;


    public override async ValueTask RenderPbgraAsync(Memory<DeviceColor> buffer) =>
        RenderPbgra(buffer, await RenderSubImagesAsync());

    private async Task<RenderedImage[]> RenderSubImagesAsync()
    {
        var renderings = new RenderedImage[componentBitmaps.Count];
        for (int i = 0; i < componentBitmaps.Count; i++)
        {
            var pixels = new DeviceColor[componentBitmaps[i].TotalPixels()];
            await componentBitmaps[i].RenderPbgraAsync(pixels);
            renderings[i] = new RenderedImage(componentBitmaps[i].Width , pixels);
        }

        return renderings;
    }

    private unsafe void RenderPbgra(Memory<DeviceColor> buffer, RenderedImage[] renderings)
        => CopyInterlacedRows(renderings, buffer);

    private unsafe void CopyInterlacedRows(RenderedImage[] renderings, Memory<DeviceColor> target)
    {
        for (int row = 0; row < Height; row++)
        {
            foreach (var rowSegment in renderings)
            {
                rowSegment.CopySegmentTo(ref target, row);
            }
        }
    }
}

internal readonly partial struct RenderedImage
{
    [FromConstructor] private readonly int Width;
    [FromConstructor] private readonly DeviceColor[] Data;

    public void CopySegmentTo(ref Memory<DeviceColor> target, int row)
    {
        Data.AsSpan(row * Width, Width).CopyTo(target.Span);
        target = target[Width..];
    }
}