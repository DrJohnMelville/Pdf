using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Information about a single profile, combined with others to make an ICC profile.
/// </summary>
/// <param name="Manufacturer">Manufacturer signature</param>
/// <param name="Device">Device signature</param>
/// <param name="DeviceAttributes">Device attributes from the underlying profile</param>
/// <param name="DeviceTechnology">Rendering technology such as CRT, dye sublimation, and etc</param>
/// <param name="ManufacturerName">Displayable version of the manufacturer name</param>
/// <param name="DeviceName">Displayable version of the device name</param>
public record struct ProfileSequenceDescriptionElement(
    uint Manufacturer,
    uint Device,
    DeviceAttributes DeviceAttributes,
    DeviceTechnology DeviceTechnology,
    MultiLocalizedUnicodeTag ManufacturerName,
    MultiLocalizedUnicodeTag DeviceName
);

/// <summary>
/// ICC tag class representing a series of transforms combined to make the current ICC transform 
/// </summary>
public class ProfileSequenceDescriptionTag 
{
    /// <summary>
    /// List of information about the profiles that were combined to make this profile.
    /// </summary>
    public IReadOnlyList<ProfileSequenceDescriptionElement> Profiles { get; }
    internal ProfileSequenceDescriptionTag(ref SpanParsingBuffer reader)
    {
        reader.Skip32BitPad();
        var profiles = new ProfileSequenceDescriptionElement[reader.GetUInt32BigEndian()];
        for (int i = 0; i < profiles.Length; i++)
        {
            profiles[i] = ParseSingleProfile(ref reader);
        }
        Profiles = profiles;
    }

    private ProfileSequenceDescriptionElement ParseSingleProfile(ref SpanParsingBuffer reader)
    {
        var mfr = reader.GetUInt32BigEndian();
        var device = reader.GetUInt32BigEndian();
        var deviceAttr = (DeviceAttributes)reader.GetUInt64BigEndian();
        var decTech = (DeviceTechnology)reader.GetUInt32BigEndian();
        return new ProfileSequenceDescriptionElement(
            mfr,
            device,
            deviceAttr,
            decTech,
            MultiLocalizedUnicodeTag.ReadFromMiddleOfStream(ref reader),
            MultiLocalizedUnicodeTag.ReadFromMiddleOfStream(ref reader));
    }
    
}