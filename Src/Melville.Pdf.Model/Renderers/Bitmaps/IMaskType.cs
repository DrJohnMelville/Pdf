namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IMaskType
{
    unsafe byte AlphaForByte(byte alpha, in byte* maskPixel);
}

internal readonly struct HardMask : IMaskType
{
    public unsafe byte AlphaForByte(byte alpha, in byte* maskPixel) =>
        maskPixel[3] == 255 ? alpha : (byte)0;
}

internal readonly struct SoftMask : IMaskType
{
    public unsafe byte AlphaForByte(byte alpha, in byte* maskPixel) => *maskPixel;
}