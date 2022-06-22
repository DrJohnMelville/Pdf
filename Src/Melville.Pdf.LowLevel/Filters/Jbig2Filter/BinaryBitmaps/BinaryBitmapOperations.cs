namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public static class BinaryBitmapOperations
{
    public static bool NoBytesLeftInRow(this IBinaryBitmap bitmap, int row, int col) => 
        row < 0 || row >= bitmap.Height || col >= bitmap.Width;
    
}