using System;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public partial class OffsetBitmap
{
    public static IBinaryBitmap Create(IBinaryBitmap inner, int y, int x) => 
        Create(inner, y, x, Math.Max(0, inner.Height - y), Math.Max(0, inner.Width - x));

    public static IBinaryBitmap Create(IBinaryBitmap inner, int y, int x, int height, int width) =>
        (x == 0 && y == 0 && height == inner.Height && width == inner.Width)
            ? inner
            : MandatoryCreate(inner, y, x, height, width);
    public static BinaryBitmaps.OffsetBitmap MandatoryCreate(IBinaryBitmap inner, int y, int x, int height, int width) =>
        (inner.ContainsPixel(y, x) && inner.ContainsPixel(y + height, x + width))
            ? new BinaryBitmaps.OffsetBitmap(inner, y, x, height, width)
            : new OffsetIncompleteBitmap(inner, y, x, height, width);

}
