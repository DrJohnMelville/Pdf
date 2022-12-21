namespace Melville.Icc.Model;

/// <summary>
/// Attributes of a device targeted by an ICC profile   
/// </summary>
[Flags]
public enum DeviceAttributes : ulong
{
    Transparent = 1,
    Matte = 2,
    Negative = 4,
    BlackANdWhite = 8
}