﻿using System.Buffers;
using System.Text;
using Melville.Icc.Parser;
using Melville.Parsing.SequenceReaders;

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

    internal MultiLocalizedUnicodeTag(ref SequenceReader<byte> reader)
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

    internal static MultiLocalizedUnicodeTag ReadFromMiddleOfStream(ref SequenceReader<byte> reader)
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