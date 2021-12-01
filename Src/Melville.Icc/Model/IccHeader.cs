namespace Melville.Icc.Model;

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