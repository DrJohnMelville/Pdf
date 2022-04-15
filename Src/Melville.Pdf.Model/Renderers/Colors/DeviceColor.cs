using System;
using System.Drawing;

namespace Melville.Pdf.Model.Renderers.Colors;

public readonly record struct DeviceColor(byte RedByte, byte GreenByte, byte BlueByte, byte Alpha)
{
    public DeviceColor(double red, double green, double blue) : this(ToByte(red), ToByte(green), ToByte(blue), 255)
    {
    }
    public DeviceColor(double red, double green, double blue, double alpha) : 
        this(ToByte(red), ToByte(green), ToByte(blue), ToByte(alpha))
    {
    }

    private static byte ToByte(double d) => (byte)(d*255);

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