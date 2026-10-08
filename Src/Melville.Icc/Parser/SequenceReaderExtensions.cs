using System.Buffers;
using Melville.Icc.Model;
using Melville.Parsing.SequenceReaders;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Icc.Parser;

internal static class IccSequenceReaderExtensions
{
    public static XyzNumber ReadXyzNumber(this ref SpanParsingBuffer source) =>
        new(source.Reads15Fixed16(), source.Reads15Fixed16(), source.Reads15Fixed16());


    public static XyzNumber ReadXyzFromUint16s(this ref SpanParsingBuffer source) =>
        new(source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1());

    public static float ReadUshortasFloat0To1(this ref SpanParsingBuffer source) => 
        source.GetUInt16BigEndian() / ((float)ushort.MaxValue);

    public static DateTime ReadDateTimeNumber(this ref SpanParsingBuffer reader) => new(
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        reader.GetUInt16BigEndian(),
        0, DateTimeKind.Utc);

}
