using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.Bitmaps;


internal abstract partial class MaskAdjuster : IPdfBitmap
{
    [FromConstructor][DelegateTo] private readonly IPdfBitmap innerBitmap;
    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        return new ValueTask(
            innerBitmap.RenderPbgra(buffer).AsTask()
                .ContinueWith(ApplyFilter, (IntPtr)buffer));
    }
    private unsafe void ApplyFilter(Task _, object targetPtr)
    {
        byte* target = (byte*)(IntPtr)targetPtr;
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

internal readonly partial struct SameSizeMaskAdjuster<TMaskType>: MaskAdjuster 
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
        var totalPixels = Width * Height * 4;
        fixed (byte* maskBytes = mask.Mask)
        {
            byte* end = maskBytes + totalPixels;
            for (byte* i = maskBytes; i < end; i+=4)
            {
                ApplyAlphaToTarget(
                    maskType.AlphaForByte(target[3], new ReadOnlySpan<byte>(i,4)),
                    ref target);
            }

        }
    }
}

internal partial class MaskAdjuster<TBitmapFilter> : MaskAdjuster 
    where TBitmapFilter : IBitmapFilter
{
    [FromConstructor] private readonly TBitmapFilter filter;


    protected override unsafe void AdjustTarget(byte* target)
    {
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                ApplyAlphaToTarget(filter.AlphaForByte(target, row, col), ref target);
            }
        }
    }

}
