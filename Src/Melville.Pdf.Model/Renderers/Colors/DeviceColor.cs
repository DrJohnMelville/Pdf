using System;
using System.Drawing;

namespace Melville.Pdf.Model.Renderers.Colors;

/// <summary>
/// Melville.PDF's primary objective is to render to the screen.  Screen colors are represented
/// by four bytes representing Red, Green, Blue, and Alpha in a SRGB colorspace.
/// </summary>
/// <param name="RedByte">A byte representing the red component</param>
/// <param name="GreenByte">A byte representing the green component</param>
/// <param name="BlueByte">A byte representing the blue component</param>
/// <param name="Alpha">A byte representing transparency 0 = fully transparent, 255 = fully opaque</param>
public readonly record struct DeviceColor(byte RedByte, byte GreenByte, byte BlueByte, byte Alpha)
{
    /// <summary>
    /// Create a device color using doubles in the range of 0-1 to represent the four components
    /// </summary>
    /// <param name="red">The red component (0-1)</param>
    /// <param name="green">The green component (0-1)</param>
    /// <param name="blue">The blue component (0-1)</param>
    /// <param name="alpha">The tranparency. 0 = Transparent - 1 = Opaque</param>
    /// <returns>A DeviceColor Corresponding to the given components</returns>
    public static DeviceColor FromDoubles(double red, double green, double blue, double alpha = 1.0) =>
        new(ToByte(red), ToByte(green), ToByte(blue), ToByte(alpha));

    private static byte ToByte(double d) => d switch
    {
        >= 1 => 255,
        <= 0 => 0,
        _ => (byte)(d * 255)
    };
    
    /// <summary>
    /// A device color that is fully transparent
    /// </summary>
    public static DeviceColor Invisible => new (0,0,0,0);
    /// <summary>
    /// A device color that is opaque black
    /// </summary>
    public static DeviceColor Black => new(0, 0, 0, 255);

    /// <summary>
    /// Premultiply the color components by the alpha value
    /// </summary>
    /// <returns></returns>
    public DeviceColor AsPreMultiplied() =>
        Alpha switch
        {
            255 => this,
            0 => Invisible,
            _ => new DeviceColor(PreMul(RedByte), PreMul(GreenByte), PreMul(BlueByte), Alpha)
        };

    private byte PreMul(byte value) => (byte)((value * Alpha) / 255);

    /// <summary>
    /// Pack the bytes into a uint packed 4 byte value.
    /// </summary>
    /// <returns>A 4 byte ARGB packed value.</returns>
    public uint AsArgbUint32() => (uint)
        ((Alpha << 24) | (RedByte << 16) | (GreenByte << 8) | BlueByte);
}