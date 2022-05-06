using System.Buffers;
using Melville.INPC;

namespace Melville.Parsing.SequenceReaders;

public static  partial class SequenceReaderExtensions
{
    [MacroItem("byte", "Uint8", 1)]
    [MacroItem("short", "Int16", 2)]
    [MacroItem("ushort", "Uint16", 2)]
    [MacroItem("int", "Int32", 4)]
    [MacroItem("uint", "Uint32", 4)]
    [MacroItem("long", "Int64", 8)]
    [MacroItem("ulong", "Uint64", 8)]
    [MacroCode("public static ~0~ ReadBigEndian~1~(this ref SequenceReader<byte> reader) => (~0~)reader.ReadBigEndianUint(~2~);")]
    [MacroCode(@"public static bool TryReadBigEndian~1~(this ref SequenceReader<byte> reader, out ~0~ ret) 
        {
             if (TryReadBigEndian(ref reader, out var inner, ~2~))
            {
                ret = (~0~)inner;
                return true;
            }
            ret = 0;
            return false;
        }")]
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
    public static float Readu8Fixed8(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianUint8()) + (((float)source.ReadBigEndianUint8()) / ((1 << 8) - 1));
    
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
    public static float[] ReadIEEE754FloatArray(
        this ref SequenceReader<byte> reader, int len, int spaceInFront = 0)
    {
        var ret = new float[len + spaceInFront];
        for (int i = spaceInFront; i < ret.Length; i++)
        {
            ret[i] = ReadIEEE754Float(ref reader);
        }
        return ret;
    }

    public static float ReadIEEE754Float(this ref SequenceReader<byte> reader) => 
        BitConverter.UInt32BitsToSingle(reader.ReadBigEndianUint32());

    public static void Skip32BitPad(ref this SequenceReader<byte> reader) => reader.Advance(4);
    public static void Skip16BitPad(ref this SequenceReader<byte> reader) => reader.Advance(2);
    public static void Skip8BitPad(ref this SequenceReader<byte> reader) => reader.Advance(1);
}