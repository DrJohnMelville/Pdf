using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public class HorizontalStripBitmap : IBinaryBitmap
{
    private IBinaryBitmap inner;
    public int Height => inner.Height;
    public int Width { get; }
    private readonly int firstCol;

    public HorizontalStripBitmap(IBinaryBitmap inner, int firstCol, int width)
    {
        this.inner = inner;
        this.firstCol = firstCol;
        Width = width;
    }

    public bool this[int row, int column]
    {
        get => inner[row, column+firstCol];
        set => inner[row, column+firstCol] = value;
    }

    public int Stride => inner.Stride;
    public (byte[], BitOffset) ColumnLocation(int column) => inner.ColumnLocation(column + firstCol);
    public bool AllIncludedPointsExist() => true;
}