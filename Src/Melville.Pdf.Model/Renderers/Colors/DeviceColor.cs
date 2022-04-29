using System;
using System.Drawing;

namespace Melville.Pdf.Model.Renderers.Colors;

public readonly record struct DeviceColor(byte RedByte, byte GreenByte, byte BlueByte, byte Alpha)
{
    public static DeviceColor FromDoubles(double red, double green, double blue, double alpha = 1.0) =>
        new(ToByte(red), ToByte(green), ToByte(blue), ToByte(alpha));

    private static byte ToByte(double d) => d switch
    {
        >= 1 => 255,
        <= 0 => 0,
        _ => (byte)(d * 255)
    };
    
    public static readonly DeviceColor Invisible = new (0,0,0,0);
    public static readonly DeviceColor Black = new(0, 0, 0, 255);

    public byte PreMulRed() => PreMul(RedByte);
    public byte PreMulGreen() => PreMul(GreenByte);
    public byte PreMulBlue() => PreMul(BlueByte);

    public DeviceColor AsPreMultiplied() =>
        Alpha switch
        {
            255 => this,
            0 => Invisible,
            _ => new DeviceColor(PreMulRed(), PreMulGreen(), PreMulBlue(), Alpha)
        };

    private byte PreMul(byte value) => (byte)((value * Alpha) / 255);
}