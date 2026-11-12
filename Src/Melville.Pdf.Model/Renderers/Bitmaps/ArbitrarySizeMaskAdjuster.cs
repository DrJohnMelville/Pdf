using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;
using System;

namespace Melville.Pdf.Model.Renderers.Bitmaps;



internal partial class ArbitrarySizeMaskAdjuster<TMaskType> : MaskAdjuster 
    where TMaskType:IMaskType
{
    [FromConstructor] private readonly MaskBitmap mask;
    [FromConstructor] private readonly TMaskType maskType;

    protected override unsafe void AdjustTarget(Memory<DeviceColor> target)
    {
        int targetPos = 0;
        var targetSpan = target.Span;
        for (int row = 0; row < Height; row++)
        {
            int rowPtr = mask.PixelOffset(MaskRow(row), 0);
            int maskRow = MaskRow(row);
            for (int col = 0; col < Width; col++)
            {
                var maskLocation = rowPtr + MaskCol(col);
                ref var tp = ref targetSpan[targetPos++];
                ApplyAlphaTo(
                    maskType.AlphaForByte(tp.Alpha, mask.PixelAt(maskLocation))
                    , ref tp);
            }
        }
    }

    private int MaskCol(int col) => col*mask.Width / Width;
    private int MaskRow(int row) => row *  mask.Height / Height;
}