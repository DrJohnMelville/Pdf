using System.Buffers;
using Melville.Icc.Model;

namespace Melville.Icc.Parser;

public static class SequenceReaderExtensions
{
    public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out ulong value, int byteCount)
    {
        value = 0;
        for (int i = 0; i < byteCount; i++)
        {
            value <<= 8;
            if (!reader.TryRead(out var oneByte)) return false;
            value |= oneByte;
        }

        return true;
    }

    public static UInt32 ReadBigEndianUint32(this ref SequenceReader<byte> reader) =>
        (uint)reader.ReadBigEndianUint(4);
    public static UInt64 ReadBigEndianUint64(this ref SequenceReader<byte> reader) =>
        reader.ReadBigEndianUint(8);
    public static short ReadBigEndianInt16(this ref SequenceReader<byte> reader) =>
        (short)reader.ReadBigEndianUint(2);
    public static ushort ReadBigEndianUint16(this ref SequenceReader<byte> reader) =>
        (ushort)reader.ReadBigEndianUint(2);
    
    public static ulong ReadBigEndianUint(this ref SequenceReader<byte> reader, int bytes)
    {
        if (!reader.TryReadBigEndian(out var ret, bytes))
            throw new InvalidDataException("Not enough input");
        return ret;
    }

    public static float Reads15Fixed16(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianInt16()) + (((float)source.ReadBigEndianUint16()) / ((1 << 16) - 1));

    public static XyzNumber ReadXyzNumber(this ref SequenceReader<byte> source) =>
        new XyzNumber(source.Reads15Fixed16(), source.Reads15Fixed16(), source.Reads15Fixed16());

    public static DateTime ReadDateTimeNumber(this ref SequenceReader<byte> reader) => new(
            reader.ReadBigEndianUint16(), 
            reader.ReadBigEndianUint16(), 
            reader.ReadBigEndianUint16(), 
            reader.ReadBigEndianUint16(), 
            reader.ReadBigEndianUint16(), 
            reader.ReadBigEndianUint16(), 
            0, DateTimeKind.Utc);
}