using System.Buffers;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;

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

    public static byte ReadBigEndianUint8(this ref SequenceReader<byte> reader) =>
        (byte)reader.ReadBigEndianUint(1);
    public static short ReadBigEndianInt16(this ref SequenceReader<byte> reader) =>
        (short)reader.ReadBigEndianUint(2);
    public static ushort ReadBigEndianUint16(this ref SequenceReader<byte> reader) =>
        (ushort)reader.ReadBigEndianUint(2);
    public static UInt32 ReadBigEndianUint32(this ref SequenceReader<byte> reader) =>
        (uint)reader.ReadBigEndianUint(4);
    public static UInt64 ReadBigEndianUint64(this ref SequenceReader<byte> reader) =>
        reader.ReadBigEndianUint(8);
    
    public static ulong ReadBigEndianUint(this ref SequenceReader<byte> reader, int bytes)
    {
        if (!reader.TryReadBigEndian(out var ret, bytes))
            throw new InvalidDataException("Not enough input");
        return ret;
    }

    public static float Reads15Fixed16(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianInt16()) + (((float)source.ReadBigEndianUint16()) / ((1 << 16) - 1));
    public static float Readu16Fixed16(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianUint16()) + (((float)source.ReadBigEndianUint16()) / ((1 << 16) - 1));

    public static XyzNumber ReadXyzNumber(this ref SequenceReader<byte> source) =>
        new XyzNumber(source.Reads15Fixed16(), source.Reads15Fixed16(), source.Reads15Fixed16());

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

    public static string ReadFixedAsciiString(this ref SequenceReader<byte> reader, int length)
    {
        Span<char> buffer= stackalloc char[length];
        int endPoos = length;
        for (int i = 0; i < length; i++)
        {
            buffer[i] = (char)reader.ReadBigEndianUint8();
            if (i < endPoos && buffer[i] == 0) endPoos = i;
        }
        return new string(buffer[..endPoos]);
    }

    public static ushort[] ReadUshortArray(this ref SequenceReader<byte> reader, int len)
    {
        if (len == 0) return Array.Empty<ushort>();
        var ret = new ushort[len];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = reader.ReadBigEndianUint16();
        }
        return ret;
    }

    public static float[] ReadScaledFloatArray(
        this ref SequenceReader<byte> reader, int len, int bytesPerSample)
    {
        float divisor = (float)(1 << (8 * bytesPerSample)) - 1;
        var ret = new float[len];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = reader.ReadBigEndianUint(bytesPerSample)/divisor;
        }
        return ret;
    }

    public static void Skip32BitPad(ref this SequenceReader<byte> reader) => reader.ReadBigEndianUint32();
    public static void Skip16BitPad(ref this SequenceReader<byte> reader) => reader.ReadBigEndianUint16();
    public static void Skip8BitPad(ref this SequenceReader<byte> reader) => reader.ReadBigEndianUint8();
}