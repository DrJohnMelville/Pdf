using Melville.JBig2.Segments;

namespace Melville.JBig2.BinaryBitmaps;

public interface IBitmapCopyTarget : IBinaryBitmap
{
    void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp);
}