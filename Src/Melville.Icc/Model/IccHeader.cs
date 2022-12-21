namespace Melville.Icc.Model;

/// <summary>
/// ICC Header data structure.
/// </summary>
/// <param name="Size">Size of the header structure</param>
/// <param name="CmmType">Desired Color Management Module type, as indicated in the ICC CMM registry</param>
/// <param name="Version">ICC profile version</param>
/// <param name="ProfileClass">The type of profile represented by this class</param>
/// <param name="DeviceColorSpace">Colorspace on the device side of the profile</param>
/// <param name="ProfileConnectionColorSpace">Colorspace of the profile connection space -- shall by XYZ or LAB</param>
/// <param name="CreatedDate">Date the profile was created</param>
/// <param name="Signature">shall containg 0x61637379 -- signature for profile files</param>
/// <param name="Platform">The computer platform on which the profile was created</param>
/// <param name="ProfileFlags">Bitfield for ICC profile options</param>
/// <param name="Manufacturer">Signature for the device manufacturer, as listed in the ICC registry</param>
/// <param name="Device">Signature for the device, as listed in the ICC registry</param>
/// <param name="DeviceAttributes">Bitfield describing the media used in this device</param>
/// <param name="RenderIntent">The default rendering intent to be used when combining with other profiles</param>
/// <param name="Illuminant">CIE values for the profile connection space illuminant, which must be CIE D50 X=0.964 Y = 1.0 Z - 0.8249</param>
/// <param name="Creator">ICC registry signature of the company that created the profile.</param>
/// <param name="ProfileIdHigh">High bits of the profile ID, which is a MD5 hash of the profile with some bits zeroed.</param>
/// <param name="ProfileIdLow">Low bits of the profile ID, which is a MD5 hash of the profile with some bits zeroed.</param>
//The order of fields in this type is their order in the header, which facilitates parsing.
public record struct IccHeader(
    uint Size,
    uint CmmType,
    uint Version,
    ProfileClass ProfileClass,
    ColorSpace DeviceColorSpace,
    ColorSpace ProfileConnectionColorSpace,
    DateTime CreatedDate,
    uint Signature,
    uint Platform,
    ProfileFlags ProfileFlags,
    uint Manufacturer,
    uint Device,
    DeviceAttributes DeviceAttributes,
    RenderIntent RenderIntent,
    XyzNumber Illuminant,
    uint Creator,
    ulong ProfileIdHigh,
    ulong ProfileIdLow
);