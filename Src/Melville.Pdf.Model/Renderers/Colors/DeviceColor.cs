using System;

namespace Melville.Pdf.Model.Renderers.Colors;

public readonly record struct DeviceColor(double Red, double Green, double Blue)
{
    public static readonly DeviceColor Invisible = new (double.NaN, double.NaN, double.NaN);
    public static readonly DeviceColor Black = new(0, 0, 0);

    public byte RedByte => (byte)(255 * Red);
    public byte GreenByte => (byte)(255 * Green);
    public byte BlueByte => (byte)(255 * Blue);

    public bool IsInvisible => double.IsNaN(Red);
}