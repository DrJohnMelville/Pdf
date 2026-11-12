using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;



internal partial class ArbitrarySizeMaskAdjuster<TMaskType> : MaskAdjuster 
    where TMaskType:IMaskType
{
    [FromConstructor] private readonly MaskBitmap mask;
    [FromConstructor] private readonly TMaskType maskType;

    protected override unsafe void AdjustTarget(byte* target)
    {
        for (int row = 0; row < Height; row++)
        {
            int rowPtr = mask.PixelOffset(MaskRow(row), 0);
            int maskRow = MaskRow(row);
            for (int col = 0; col < Width; col++)
            {
                var maskLocation = rowPtr + MaskCol(col);
                ApplyAlphaToTarget(
                    maskType.AlphaForByte(target[3], mask.PixelAt(maskLocation))
                    , ref target);
            }
        }
    }

    private int MaskCol(int col) => col*mask.Width / Width;
    private int MaskRow(int row) => row *  mask.Height / Height;
}