using System.Buffers;
using System.Text;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public record struct SingleUincodeString(ushort Langugae, ushort Country, string Value);

public class MultiLocalizedUnicodeTag: ProfileData
{
    public IReadOnlyList<SingleUincodeString> Encodings { get; }

    public MultiLocalizedUnicodeTag(ref SequenceReader<byte> reader)
    {
        reader.Skip32BitPad();
        var length = reader.ReadBigEndianUint32();
        var recordLen = reader.ReadBigEndianUint32();
        var array = new SingleUincodeString[length];
        for (int i = 0; i < length; i++)
        {
            var language = reader.ReadBigEndianUint16();
            var country = reader.ReadBigEndianUint16();
            var stringLength = reader.ReadBigEndianUint32();
            var offset = reader.ReadBigEndianUint32();
            if (recordLen > 12) reader.Advance(recordLen - 12);
            var stringSlice = reader.Sequence.Slice(offset, stringLength);
            var str = Encoding.BigEndianUnicode.GetString(stringSlice);
            array[i] = new SingleUincodeString(language, country, str);
        }

        Encodings = array;
    }
}