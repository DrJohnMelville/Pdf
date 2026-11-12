using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal interface IMaskType
{
    unsafe byte AlphaForByte(byte alpha, DeviceColor maskPixel);
}

internal readonly struct HardMask : IMaskType
{
    public unsafe byte AlphaForByte(byte alpha, DeviceColor maskPixel) =>
        maskPixel.Alpha == 255 ? alpha : (byte)0;
}

internal readonly struct SoftMask : IMaskType
{
    public unsafe byte AlphaForByte(byte alpha, DeviceColor maskPixel) => maskPixel.RedByte;
}