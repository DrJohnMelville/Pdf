namespace Melville.Icc.Model;

[Flags]
public enum DeviceAttributes : ulong
{
    Transparent = 1,
    Matte = 2,
    Negative = 4,
    BlackANdWhite = 8
}