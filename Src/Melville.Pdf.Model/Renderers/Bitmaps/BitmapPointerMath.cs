using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public static class BitmapPointerMath
{
    public static unsafe void PushPixel(ref byte* output, in DeviceColor color)
    {
        *output++ = color.BlueByte;
        *output++ = color.GreenByte;
        *output++ = color.RedByte;
        *output++ = color.Alpha;
    }
}