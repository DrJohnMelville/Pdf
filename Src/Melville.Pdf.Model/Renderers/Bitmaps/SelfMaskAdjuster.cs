using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal partial class SelfMaskAdjuster<TMaskType>: MaskAdjuster 
    where TMaskType : IMaskType
{
    [FromConstructor] private readonly TMaskType maskType;

    protected override unsafe void AdjustTarget(System.Memory<DeviceColor> target)
    {
        var targetSpan = target.Span;
        for (int i = 0; i < target.Length; i++)
        {
            ApplyAlphaTo(maskType.AlphaForByte(0, targetSpan[i]), ref targetSpan[i]);
        }
    }
}