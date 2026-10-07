using System.Buffers;
using Melville.Icc.Model;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Parser;

internal static class IccSequenceReaderExtensions
{
    [Obsolete("Get rid of this when IByteSource Goes away")]
    public static XyzNumber ReadXyzNumber(this ref SequenceReader<byte> source) =>
        new(source.Reads15Fixed16(), source.Reads15Fixed16(), source.Reads15Fixed16());
    public static XyzNumber ReadXyzNumber(this in SpanParsingBuffer source) =>
        new(source.GetUInt16BigEndian(), source.GetUInt16BigEndian(), source.GetUInt16BigEndian());

    public static XyzNumber ReadXyzFromUint16s(this ref SequenceReader<byte> source) =>
        new(source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1());

    public static float ReadUshortasFloat0To1(this ref SequenceReader<byte> source) => 
        source.ReadBigEndianUint16() / ((float)ushort.MaxValue);

    [Obsolete("Get rid of this when IByteSource Goes away")]
    public static DateTime ReadDateTimeNumber(this ref SequenceReader<byte> reader) => new(
        reader.ReadBigEndianUint16(),
        reader.ReadBigEndianUint16(),
        reader.ReadBigEndianUint16(),
        reader.ReadBigEndianUint16(),
        reader.ReadBigEndianUint16(),
        reader.ReadBigEndianUint16(),
        0, DateTimeKind.Utc);

    public static DateTime ReadDateTimeNumber(this in SpanParsingBuffer reader) => new(
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        0, DateTimeKind.Utc);

}