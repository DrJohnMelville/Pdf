using System.Buffers;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Record describing an ICC profile sequence member
/// </summary>
/// <param name="ProfileIdHigh">High 8 bytes of the profile id</param>
/// <param name="ProfileIdLow">Low 8 bytes of the profile id</param>
/// <param name="Description">Name of the profile</param>
public record struct ProfileIdentifier(
    ulong ProfileIdHigh, 
    ulong ProfileIdLow, 
    MultiLocalizedUnicodeTag Description);

/// <summary>
/// Tag describing a sequence of profiles
/// </summary>
public class ProfileSequenceIdentifierTag 
{
    /// <summary>
    /// Profiles that comprise the sequence
    /// </summary>
    public IReadOnlyList<ProfileIdentifier> Profiles { get;}

    internal ProfileSequenceIdentifierTag(ref SequenceReader<byte> reader)
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