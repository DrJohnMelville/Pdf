using System.Buffers;
using System.Text;
using Melville.Icc.Parser;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Model.Tags;

/// <summary>
/// Represents a string localized to a specific language and culture.
/// </summary>
/// <param name="Language">Language code as specified in ISO 639-1</param>
/// <param name="Country">Country code as specified in ISO 3166-1</param>
/// <param name="Value">String value as Unicode UTF-16BE</param>
public record struct SingleUincodeString(ushort Language, ushort Country, string Value);

/// <summary>
/// An ICC block which contains a unicode string which may be localized to multiple language/country pairs.
/// </summary>
public class MultiLocalizedUnicodeTag
{
    /// <summary>
    /// The versions of the striung in multiple language/couontry pairs
    /// </summary>
    public IReadOnlyList<SingleUincodeString> Encodings { get; }

    internal MultiLocalizedUnicodeTag(ref SpanParsingBuffer reader)
    {
        reader.VerifyInCorrectPositionForTagRelativeOffsets();
        reader.Skip32BitPad();
        var length = reader.GetUInt32BigEndian();
        var recordLen = reader.GetUInt32BigEndian();
        var array = new SingleUincodeString[length];
        long max = 0;
        for (int i = 0; i < length; i++)
        {
            var language = reader.GetUInt16BigEndian();
            var country = reader.GetUInt16BigEndian();
            var stringLength = reader.GetUInt32BigEndian();
            var offset = reader.GetUInt32BigEndian();
            max = Math.Max(max, offset + stringLength);
            if (recordLen > 12) reader.Advance((int)recordLen - 12);
            var subReader = reader.ReaderAt(offset);

            //var stringSlice = reader.Sequence.Slice(offset, stringLength);
            var str = Encoding.BigEndianUnicode.GetString(subReader.Peek()[..(int)stringLength]);
            array[i] = new SingleUincodeString(language, country, str);
        }

        reader.AdvanceToRelativePosition(max);
        Encodings = array;
    }

    internal static MultiLocalizedUnicodeTag ReadFromMiddleOfStream(ref SpanParsingBuffer reader)
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
        if (reader.GetUInt32BigEndian() != IccTags.mluc)
            throw new InvalidDataException(
                "Should have a MultiLocalizedUnicode record in profile sequence description");
    }
    private static void ResetCurrentPointToBeginningOfReader(ref SpanParsingBuffer reader)
    {
        reader.RelativePosition = 0;
    }

    private static void CheckForMultiUnicodeHeader(ref SpanParsingBuffer reader)
    {
        if (reader.GetUInt32BigEndian() != IccTags.mluc)
            throw new InvalidDataException(
                "Should have a MultiLocalizedUnicode record in profile sequence description");
    }
}