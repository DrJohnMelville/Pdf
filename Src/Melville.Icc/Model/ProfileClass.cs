namespace Melville.Icc.Model;

/// <summary>
/// The type of device described by this profile
/// </summary>
public enum ProfileClass : uint
{
    Input = 0x73636372,
    Display = 0x6d6e7472,
    Output = 0x70727472,
    DeviceLink = 0x6c696e6b,
    ColorSpace = 0x73706163,
    Abstract = 0x61627374,
    NamedColor = 0x6e6d636c
}