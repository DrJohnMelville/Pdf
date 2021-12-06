using System.Buffers;
using System.Text;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct SingleUincodeString(ushort Langugae, ushort Country, string Value);

public class MultiLocalizedUnicodeTag
{
    public IReadOnlyList<SingleUincodeString> Encodings { get; }

    public MultiLocalizedUnicodeTag(ref SequenceReader<byte> reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var length = reader.ReadBigEndianUint32();
        var recordLen = reader.ReadBigEndianUint32();
        var array = new SingleUincodeString[length];
        long max = 0;
        for (int i = 0; i < length; i++)
        {
            var language = reader.ReadBigEndianUint16();
            var country = reader.ReadBigEndianUint16();
            var stringLength = reader.ReadBigEndianUint32();
            var offset = reader.ReadBigEndianUint32();
            max = Math.Max(max, offset + stringLength);
            if (recordLen > 12) reader.Advance(recordLen - 12);
            var stringSlice = reader.Sequence.Slice(offset, stringLength);
            var str = Encoding.BigEndianUnicode.GetString(stringSlice);
            array[i] = new SingleUincodeString(language, country, str);
        }

        reader = new SequenceReader<byte>(reader.Sequence.Slice(max));
        Encodings = array;
    }

    public static MultiLocalizedUnicodeTag ReadFromMiddleOfStream(ref SequenceReader<byte> reader)
    {
        ResetCurrentPointToBeginningOfReader(ref reader);
        CheckForMultiUnicodeHeader(ref reader);
        return new MultiLocalizedUnicodeTag(ref reader);
    }

    private static void ResetCurrentPointToBeginningOfReader(ref SequenceReader<byte> reader)
    {
        reader = new SequenceReader<byte>(reader.Sequence.Slice(reader.Position));
    }

    private static void CheckForMultiUnicodeHeader(ref SequenceReader<byte> reader)
    {
        if (reader.ReadBigEndianUint32() != IccTags.mluc)
            throw new InvalidDataException(
                "Should have a MultiLocalizedUnicode record in profilesequencedescription");
    }
}