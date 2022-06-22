namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

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