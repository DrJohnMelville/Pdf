namespace Melville.JBig2.BinaryBitmaps;


public static class OffsetBitmapFactory
{
    public static IBinaryBitmap Create(IBinaryBitmap inner, int y, int x) => 
        Create(inner, y, x, Math.Max(0, inner.Height - y), Math.Max(0, inner.Width - x));

    public static IBinaryBitmap CreateHorizontalStrip(IBinaryBitmap inner, int x, int width) =>
        Create(inner, 0, x, inner.Height, width);

    public static IBinaryBitmap Create(IBinaryBitmap inner, int y, int x, int height, int width) =>
        (x == 0 && y == 0 && height == inner.Height && width == inner.Width)
            ? inner
            : new OffsetBitmap(inner, y, x, height, width);
}
