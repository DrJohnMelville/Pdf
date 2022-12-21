namespace Melville.JBig2.BinaryBitmaps;

internal static class BinaryBitmapOperations
{
    public static bool NoBytesLeftInRow(this IBinaryBitmap bitmap, int row, int col) => 
        row < 0 || row >= bitmap.Height || col >= bitmap.Width;
    
}