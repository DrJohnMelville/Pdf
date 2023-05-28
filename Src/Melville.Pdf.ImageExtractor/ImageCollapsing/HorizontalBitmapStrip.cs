using System.Diagnostics.Contracts;
using System.Numerics;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing;

internal partial class HorizontalBitmapStrip : BitmapStrip
{
    public override int Width => componentBitmaps.Sum(i => i.Width);
    public override int Height => FirstChild.Height;
    public override Vector2 PositionBottomLeft => FirstChild.PositionBottomLeft;
    public override Vector2 PositionBottomRight => LastChild.PositionBottomRight;
    public override Vector2 PositionTopLeft => FirstChild.PositionTopLeft;
    public override Vector2 PositionTopRight => LastChild.PositionTopRight;


    protected override async ValueTask RenderPbgraAsync(PointerHolder buffer) =>
        RenderPbgra(buffer, await RenderSubImagesAsync());

    private async Task<RenderedImage[]> RenderSubImagesAsync()
    {
        var renderings = new RenderedImage[componentBitmaps.Count];
        for (int i = 0; i < componentBitmaps.Count; i++)
        {
            renderings[i] = new RenderedImage(componentBitmaps[i].Width * 4,
                await componentBitmaps[i].AsByteArrayAsync().CA());
        }

        return renderings;
    }

    private unsafe void RenderPbgra(PointerHolder buffer, RenderedImage[] renderings)
        => CopyInterlacedRows(renderings, buffer.BasePointer());

    private unsafe void CopyInterlacedRows(RenderedImage[] renderings, byte* target)
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
    [FromConstructor] private readonly int RowLength;
    [FromConstructor] private readonly byte[] Data;

    public unsafe void CopySegmentTo(ref byte* target, int row)
    {
        GetRow(row).CopyTo(new Span<byte>(target, RowLength));
        target += RowLength;
    }

    [Pure]
    private Span<byte> GetRow(int row) => Data.AsSpan(row * RowLength, RowLength);
}