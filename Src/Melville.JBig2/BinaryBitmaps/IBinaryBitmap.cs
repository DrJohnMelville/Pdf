namespace Melville.JBig2.BinaryBitmaps;

public interface IBinaryBitmap
{
    int Width { get; }
    int Height { get;}
    bool this[int row, int column] {  get; set; }
    int Stride { get; }
    (byte[] Array, BitOffset Offset) ColumnLocation (int column);
    BitRowWriter StartWritingAt(int row, int column);
    bool ContainsPixel(int row, int col);
    BitmapPointer PointerFor(int row, int col);
}

public static class IBinaryBitmapOperations
{
    public static int BufferLength(this IBinaryBitmap bitmap) => bitmap.Stride * bitmap.Height;
}