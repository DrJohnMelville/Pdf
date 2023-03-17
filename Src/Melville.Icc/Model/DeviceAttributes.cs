namespace Melville.Icc.Model;

/// <summary>
/// Attributes of a device targeted by an ICC profile   
/// </summary>
[Flags]
public enum DeviceAttributes : ulong
{
    /// <summary>
    /// Device background is transparent.
    /// </summary>
    Transparent = 1,
    /// <summary>
    /// Device has a matte finish.
    /// </summary>
    Matte = 2,
    /// <summary>
    /// Device prints negatives
    /// </summary>
    Negative = 4,
    /// <summary>
    /// Device supports black and white output/
    /// </summary>
    BlackANdWhite = 8
}