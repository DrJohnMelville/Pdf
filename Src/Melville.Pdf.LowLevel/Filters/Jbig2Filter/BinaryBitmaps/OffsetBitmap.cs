using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public partial class OffsetBitmap : IBinaryBitmap
{
    protected readonly IBinaryBitmap inner;
    protected readonly int x;
    protected readonly int y;
    public int Width { get; }
    public int Height { get; }
    
    protected OffsetBitmap(IBinaryBitmap inner, int y, int x, int height, int width)
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
        get
        {
            AssertPositionIsInBitmap(row, column);
            return inner[row + y, x + column];
        }
        set
        {
            AssertPositionIsInBitmap(row, column);
            inner[row + y, x + column] = value;
        }
    }

    private void AssertPositionIsInBitmap(int row, int column) =>
        Debug.Assert(this.ContainsPixel(row, column));

    public virtual bool AllIncludedPointsExist() => true;
}