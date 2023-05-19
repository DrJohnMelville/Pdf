using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;



internal partial class ArbitrarySizeMaskAdjuster<TMaskType> : MaskAdjuster 
    where TMaskType:IMaskType
{
    [FromConstructor] private readonly MaskBitmap mask;
    [FromConstructor] private readonly TMaskType maskType;

    protected override unsafe void AdjustTarget(byte* target)
    {
        fixed (byte* maskBytes = mask.Mask)
        {
            for (int row = 0; row < Height; row++)
            {
                byte* rowPtr = maskBytes + mask.PixelOffset(MaskRow(row), 0);
                for (int col = 0; col < Width; col++)
                {
                    var maskLocation = rowPtr + (4 * MaskCol(col));
                    ApplyAlphaToTarget(
                        maskType.AlphaForByte(target[3], maskLocation)
                        , ref target);
                }
            }

        }
    }

    private int MaskCol(int col) => col*mask.Width / Width;
    private int MaskRow(int row) => row *  mask.Height / Height;
}