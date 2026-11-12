using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal partial class SelfMaskAdjuster<TMaskType>: MaskAdjuster 
    where TMaskType : IMaskType
{
    [FromConstructor] private readonly TMaskType maskType;

    protected override unsafe void AdjustTarget(byte* target)
    {
        var totalPixels = this.ReqiredBufferSize();
        byte* end = target + totalPixels;
        while (target < end)
        {
            ApplyAlphaToTarget(maskType.AlphaForByte(0, *((DeviceColor*)target)), ref target);
        }
    }
}