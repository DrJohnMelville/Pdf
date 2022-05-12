
namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public class VerticalStripBitmap : IBinaryBitmap
{
    private IBinaryBitmap inner;
    public int Height => inner.Height;
    public int Width { get; }
    private readonly int firstCol;

    public VerticalStripBitmap(IBinaryBitmap inner, int firstCol, int width)
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
}