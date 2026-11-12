using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;


internal abstract partial class MaskAdjuster : IPdfBitmap
{
    [FromConstructor][DelegateTo] private readonly IPdfBitmap innerBitmap;
    public async ValueTask RenderPbgraAsync(Memory<DeviceColor> buffer)
    {
        await innerBitmap.RenderPbgraAsync(buffer);
        AdjustOutput(buffer);
    }

    private unsafe void AdjustOutput(Memory<DeviceColor> buffer)
    {
        AdjustTarget(buffer);
    }

    protected abstract unsafe void AdjustTarget(Memory<DeviceColor> target);
    
    protected void ApplyAlphaTo(byte alpha, ref DeviceColor pixel)
    {
        switch (alpha)
        {
            case 255: break;
            case 0: pixel = new DeviceColor();
                break;
            default: pixel = pixel.PreMultipliedBy(alpha);
                break;
        }
    }
}
