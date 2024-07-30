using System.Diagnostics;
using Melville.INPC;

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

        protected override unsafe void AdjustTarget(byte* target)
        {
            var totalPixels = this.ReqiredBufferSize();
            fixed (byte* maskBytes = mask.Mask)
            {
                byte* end = maskBytes + totalPixels;
                for (byte* i = maskBytes; i < end; i+=4)
                {
                    ApplyAlphaToTarget(
                        maskType.AlphaForByte(target[3], i),
                        ref target);
                }

            }
        }
    }
}