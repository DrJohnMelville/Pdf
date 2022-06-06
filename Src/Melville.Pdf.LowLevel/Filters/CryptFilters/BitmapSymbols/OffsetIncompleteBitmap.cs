using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public partial class OffsetBitmap : IBinaryBitmap
{

    private class OffsetIncompleteBitmap : OffsetBitmap
    {
        public OffsetIncompleteBitmap(IBinaryBitmap inner, int y, int x, int height, int width) : 
            base(inner, y, x, height, width)
        {
        }

        private bool PointExists(int row, int column)
        {
            Debug.Assert(this.ContainsPixel(row, column));
            return inner.ContainsPixel(row + y, column + x);
        }
        public override bool this[int row, int column]
        {
            get => PointExists(row, column) && base[row, column];
            set
            {
                if (PointExists(row, column))
                   base[row, column] = value;
            }
        }

        public override bool AllIncludedPointsExist() => false;
    }
}