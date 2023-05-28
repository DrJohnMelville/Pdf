using System.Numerics;
using Melville.INPC;

namespace Melville.Pdf.ImageExtractor.ImageCollapsing;

internal readonly partial struct ImageColapser
{
    [FromConstructor] private readonly IList<IExtractedBitmap> extractedBitmaps;

    public void Process()
    {
        while (TryCombine(CombineDirection.Vertical) ||
               TryCombine(CombineDirection.Horizontal))
        {
        }
    }

    private bool TryCombine(CombineDirection direction)
    {
        var totalImages = extractedBitmaps.Count;
        for (int i = 0; i < totalImages; i++)
        {
            for (int j = i + 1; j < totalImages; j++)
            {
                if (TryCombineImages(i, j, direction) || 
                    TryCombineImages(j, i, direction))
                    return true;
            }
        }

        return false;
    }

    private bool TryCombineImages(int i, int j, CombineDirection direction)
    {
        if (!direction.IsBefore(extractedBitmaps[i], extractedBitmaps[j]))
            return false;

        CombineImages(i, j, direction);
        return true;

    }

    private void CombineImages(int prior, int next, CombineDirection direction)
    {
        extractedBitmaps[prior] = 
            direction.Combine(extractedBitmaps[prior], extractedBitmaps[next]);
        extractedBitmaps.RemoveAt(next);
    }

    private abstract class CombineDirection
    {
        public static readonly CombineDirection Vertical = new VerticalImpl();
        public static readonly CombineDirection Horizontal = new HorizontalImpl();

        public abstract bool IsBefore(IExtractedBitmap prior, IExtractedBitmap next);
        public abstract IExtractedBitmap Combine(
            IExtractedBitmap prior, IExtractedBitmap next);

        private class VerticalImpl : CombineDirection
        {
            public override bool IsBefore(IExtractedBitmap prior, IExtractedBitmap next)
            =>
            prior.Width == next.Width &&
            prior.Page == next.Page &&
            WithinOne(prior.PositionBottomLeft, next.PositionTopLeft) &&
            WithinOne(prior.PositionBottomRight, next.PositionTopRight);

            public override IExtractedBitmap Combine(
                IExtractedBitmap prior, IExtractedBitmap next) => 
                BitmapStrip.Combine<VerticalBitmapStrip>(prior, next);
        }

        private class HorizontalImpl : CombineDirection
        {
            public override bool IsBefore(IExtractedBitmap prior, IExtractedBitmap next)
            =>
            prior.Height == next.Height &&
            prior.Page == next.Page &&
            WithinOne(prior.PositionTopRight, next.PositionTopLeft) &&
            WithinOne(prior.PositionBottomRight, next.PositionBottomLeft);

            public override IExtractedBitmap Combine(
                IExtractedBitmap prior, IExtractedBitmap next) => 
                BitmapStrip.Combine<HorizontalBitmapStrip>(prior, next);
        }

        private static bool WithinOne(Vector2 prior, Vector2 next) =>
            WithinOne(prior.X, next.X) && WithinOne(prior.Y, next.Y);

        private static bool WithinOne(float prior, float next) =>
            Math.Abs(next - prior) < 1.0f;
    }
}