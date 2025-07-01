using System;

namespace Melville.Pdf.Model.Renderers.Colors;

/// <summary>
/// Melville.PDF's primary objective is to render to the screen.  Screen colors are represented
/// by four bytes representing Red, Green, Blue, and Alpha in a SRGB colorspace.
/// </summary>
public readonly struct DeviceColor
{
    // The ordering of these fields allows us to use a simple memory copy to copy device colors int
    // bitmaps.
    /// <summary>
    /// The blue component of the color
    /// </summary>
    public readonly byte BlueByte;
    /// <summary>
    /// The green component of the color
    /// </summary>
    public readonly byte GreenByte;
    /// <summary>
    /// The red component of the color
    /// </summary>
    public readonly byte RedByte;
    /// <summary>
    /// The Alpha component of the color. 0 == fully transparent 255 = fully opaque.
    /// </summary>
    public readonly byte Alpha;

    /// <summary>
    /// Create a new device color
    /// </summary>
    /// <param name="redByte">The red component</param>
    /// <param name="greenByte">The green component</param>
    /// <param name="blueByte">The blue component</param>
    /// <param name="alpha">The alpha component</param>
    public DeviceColor(byte redByte, byte greenByte, byte blueByte, byte alpha)
    {
        RedByte = redByte;
        GreenByte = greenByte;
        BlueByte = blueByte;
        Alpha = alpha;
    }

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

    /// <summary>
    /// Apply an alpha to a device color
    /// </summary>
    /// <param name="newAlpha">The transparency by which to adjust the color</param>
    /// <returns>A new device color with the desired alpha.</returns>
    public DeviceColor WithAlpha(double newAlpha) =>
        newAlpha < 1.0?
        new(RedByte, GreenByte, BlueByte, (byte)(1.0 * Alpha * newAlpha)):
        this;
}