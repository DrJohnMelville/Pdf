using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps
{
    internal partial class SameSizeMaskAdjuster<TMaskType>: MaskAdjuster 
        where TMaskType : IMaskType
    {
        [FromConstructor] private readonly MaskBitmap mask;
        [FromConstructor] private readonly TMaskType maskType;
#if DEBUG
    partial void OnConstructed()
    {
        Debug.Assert(mask.Height == Height);
        Debug.Assert(mask.Width == Width);
    }
#endif
        protected override unsafe void AdjustTarget(Memory<DeviceColor> target)
        {
            var targetSpan = target.Span; 
            for (int i = 0; i < targetSpan.Length; i++)
            {
                ApplyAlphaTo(mask.PixelAt(i).GreenByte, ref targetSpan[i]);
            }
        }
    }
}