using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct ProfileIdentifier(
    ulong ProfileIdHigh, 
    ulong ProfileIdLow, 
    MultiLocalizedUnicodeTag Description);

public class ProfileSequenceIdentifierTag : ProfileData
{
    public IReadOnlyList<ProfileIdentifier> Profiles { get;}

    public ProfileSequenceIdentifierTag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var profiles = new ProfileIdentifier[reader.ReadBigEndianUint32()];
        for (int i = 0; i < profiles.Length; i++)
        {
            profiles[i] = ParseProfileIdentifier(reader.ReadPositionNumber());
        }
        Profiles = profiles;
    }

    private ProfileIdentifier ParseProfileIdentifier(SequenceReader<byte> reader)
    {
        return new ProfileIdentifier(reader.ReadBigEndianUint64(), reader.ReadBigEndianUint64(),
            MultiLocalizedUnicodeTag.ReadFromMiddleOfStream(ref reader));
    }
}