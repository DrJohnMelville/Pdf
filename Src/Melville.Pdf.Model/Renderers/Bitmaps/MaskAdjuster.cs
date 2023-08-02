using System;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;


internal abstract partial class MaskAdjuster : IPdfBitmap
{
    [FromConstructor][DelegateTo] private readonly IPdfBitmap innerBitmap;
    public unsafe ValueTask RenderPbgraAsync(byte* buffer)
    {
        return new ValueTask(
            innerBitmap.RenderPbgraAsync(buffer).AsTask()
                .ContinueWith(ApplyFilter, (IntPtr)buffer));
    }
    private unsafe void ApplyFilter(Task _, object? targetPtr)
    {
        // in this case I know targetPtr is not null but the 
        byte* target = (byte*)(IntPtr)targetPtr!;
        AdjustTarget(target);
    }

    protected abstract unsafe void AdjustTarget(byte* target);

    protected unsafe void ApplyAlphaToTarget(byte alpha, ref byte* target)
    {
        switch (alpha)
        {
            case 255: break;
            case 0:
                target[0] = 0;
                target[1] = 0;
                target[2] = 0;
                target[3] = 0;
                break;
            default:
                Premultiply(ref target[0], alpha);
                Premultiply(ref target[1], alpha);
                Premultiply(ref target[2], alpha);
                target[3] = alpha;
                break;
        }
        target += 4;
    }

    private void Premultiply(ref byte b, byte alpha) => b = (byte)((b * alpha) >> 8);
}
