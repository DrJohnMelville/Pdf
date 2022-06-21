using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

[DebuggerDisplay("{this.BitmapString()}")]
public partial class OffsetBitmap : IBinaryBitmap
{
    private readonly IBinaryBitmap inner;
    private readonly int x;
    private readonly int y;
    public int Width { get; }
    public int Height { get; }

    public OffsetBitmap(IBinaryBitmap inner, int y, int x, int height, int width)
    {
        this.inner = inner;
        this.x = x;
        this.y = y;
        Width = width;
        Height = height;
    }

    public int Stride => inner.Stride;
    public (byte[] Array, BitOffset Offset) ColumnLocation(int column)
    {
        var (array, colOffset) = inner.ColumnLocation(column + x);
        return (array, colOffset.AddRows(y,Stride));
    }

    public virtual bool this[int row, int column]
    {
        get => inner[row + y, x + column];
        set => inner[row + y, x + column] = value;
    }

    public bool ContainsPixel(int row, int col) =>
        row >= 0 && row < Height && col >= 0 && col < Width &&
        inner.ContainsPixel(row + y, col + x);

    public BitmapPointer PointerFor(int row, int col)
    {
        return this.NoBytesLeftInRow(row, col) ?
            BitmapPointer.EmptyRow : 
            InnerPointerStartingAtColumn(row, col).SelectRowLength(col, Width);
    }

    private BitmapPointer InnerPointerStartingAtColumn(int row, int col) =>
        col < 0 ? 
            inner.PointerFor(row + y, x).WithPrefixBits(-col) : 
            inner.PointerFor(row + y, col + x);
}