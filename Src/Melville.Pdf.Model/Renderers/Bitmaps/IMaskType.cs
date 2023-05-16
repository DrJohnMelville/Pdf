using Melville.Pdf.Model.Renderers.Bitmaps;
using System;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IMaskType
{
    byte AlphaForByte(byte alpha, in ReadOnlySpan<byte> maskPixel);
}

internal readonly struct HardMask : IMaskType
{
    public byte AlphaForByte(byte alpha, in ReadOnlySpan<byte> maskPixel) =>
        maskPixel[3] == 255 ? alpha : (byte)0;
}

internal readonly struct SoftMask : IMaskType
{
    public byte AlphaForByte(byte alpha, in ReadOnlySpan<byte> maskPixel)
    {
        return maskPixel[0];
    }
}