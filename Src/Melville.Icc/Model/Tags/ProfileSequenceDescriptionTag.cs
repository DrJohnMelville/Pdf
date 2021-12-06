using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct ProfileSequenceDescriptionElement(
    uint Manufacturer,
    uint Device,
    DeviceAttributes DeviceAttributes,
    DeviceTechnology DeviceTechnology,
    MultiLocalizedUnicodeTag ManufacturerName,
    MultiLocalizedUnicodeTag DeviceName
);

public class ProfileSequenceDescriptionTag 
{
    public IReadOnlyList<ProfileSequenceDescriptionElement> Profiles { get; }
    public ProfileSequenceDescriptionTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var profiles = new ProfileSequenceDescriptionElement[reader.ReadBigEndianUint32()];
        for (int i = 0; i < profiles.Length; i++)
        {
            profiles[i] = ParseSingleProfile(ref reader);
        }
        Profiles = profiles;
    }

    private ProfileSequenceDescriptionElement ParseSingleProfile(ref SequenceReader<byte> reader)
    {
        var mfr = reader.ReadBigEndianUint32();
        var device = reader.ReadBigEndianUint32();
        var deviceAttr = (DeviceAttributes)reader.ReadBigEndianUint64();
        var decTech = (DeviceTechnology)reader.ReadBigEndianUint32();
        return new ProfileSequenceDescriptionElement(
            mfr,
            device,
            deviceAttr,
            decTech,
            MultiLocalizedUnicodeTag.ReadFromMiddleOfStream(ref reader),
            MultiLocalizedUnicodeTag.ReadFromMiddleOfStream(ref reader));
    }
    
}