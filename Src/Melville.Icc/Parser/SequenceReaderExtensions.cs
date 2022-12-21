using System.Buffers;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Parsing.SequenceReaders;

namespace Melville.Icc.Parser;

internal static class IccSequenceReaderExtensions
{
    public static XyzNumber ReadXyzNumber(this ref SequenceReader<byte> source) => 
        new(source.Reads15Fixed16(), source.Reads15Fixed16(), source.Reads15Fixed16());

    public static XyzNumber ReadXyzFromUint16s(this ref SequenceReader<byte> source) =>
        new(source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1(),
            source.ReadUshortasFloat0To1());

    public static float ReadUshortasFloat0To1(this ref SequenceReader<byte> source) => 
        source.ReadBigEndianUint16() / ((float)ushort.MaxValue);

    public static DateTime ReadDateTimeNumber(this ref SequenceReader<byte> reader) => new(
        reader.ReadBigEndianUint16(), 
        reader.ReadBigEndianUint16(), 
        reader.ReadBigEndianUint16(), 
        reader.ReadBigEndianUint16(), 
        reader.ReadBigEndianUint16(), 
        reader.ReadBigEndianUint16(), 
        0, DateTimeKind.Utc);

}