namespace Melville.Icc.Model;

/// <summary>
/// The type of device described by this profile
/// </summary>
public enum ProfileClass : uint
{
    /// <summary>
    /// Profile describes a color capture device
    /// </summary>
    Input = 0x73636372,
    /// <summary>
    /// Profile describes a CRT or LCD display device
    /// </summary>
    Display = 0x6d6e7472,
    /// <summary>
    /// Device describes a printer, plotter, or other output device
    /// </summary>
    Output = 0x70727472,
    /// <summary>
    /// Device link profile
    /// </summary>
    DeviceLink = 0x6c696e6b,
    /// <summary>
    /// Profile describes an arbitrary colorspace.
    /// </summary>
    ColorSpace = 0x73706163,
    /// <summary>
    /// Profile does not describe a specific physical device
    /// </summary>
    Abstract = 0x61627374,
    /// <summary>
    /// Profile describes named colors.
    /// </summary>
    NamedColor = 0x6e6d636c
}