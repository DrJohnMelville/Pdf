using Melville.StreamInterfaces.ParsingBuffers;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Melville.Parsing.SequenceReaders;

/// <summary>
/// Extensions for reading varioyus types out of a SpanParsingBuffer
/// </summary>
public static class SpanParserExtensions
{

    /// <summary>
    /// Read any Binary Integer type in big endian format
    /// </summary>
    /// <typeparam name="T">The type of number to read</typeparam>
    /// <param name="reader">The sequence reader to take bytes from.</param>
    /// <param name="value">Out variable that receives the output</param>
    /// <returns>True if the read is successdful, false if there is not enough data.</returns>
    public static bool TryReadBigEndian<T>(this ref SpanParsingBuffer reader, out T value)
        where T : IBinaryInteger<T> =>
        GenericIntReader<T>.TryReadBigEndian(ref reader, out value);

    /// <summary>
    /// Attempts to read a big endian value of a given number of bytes into a ulong. 
    /// </summary>
    /// <param name="reader">The source of bytes for the data.</param>
    /// <param name="value">Out parameter for the result of the parsing operation</param>
    /// <param name="byteCount">Number of bytes to read</param>
    /// <returns>True if there are enough bytes to read.  False otherwise.</returns>
    public static bool TryReadBigEndian(this ref SpanParsingBuffer reader, out ulong value, int byteCount)
    {
        value = 0;
        for (int i = 0; i < byteCount; i++)
        {
            value <<= 8;
            if (!reader.TryGetUInt8(out var oneByte)) return false;
            value |= oneByte;
        }
        return true;
    }

    /// <summary>
    /// Reads a float encoded using the fixed 15/16 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Reads15Fixed16(this ref SpanParsingBuffer source) =>
        ((float)source.GetInt16BigEndian()) + (((float)source.GetUInt16BigEndian()) / ((1 << 16) - 1));

    /// <summary>
    /// Reads a positive float encoded using the fixed 16/16 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Readu16Fixed16(this ref SpanParsingBuffer source) =>
        ((float)source.GetUInt16BigEndian()) + (((float)source.GetUInt16BigEndian()) / ((1 << 16) - 1));

    /// <summary>
    /// Reads a float encoded using the fixed 8/8 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Readu8Fixed8(this ref SpanParsingBuffer source) =>
        ((float)source.GetUInt8()) + (((float)source.GetUInt8()) / ((1 << 8) - 1));

    /// <summary>
    /// Read a sequence of bytes as an ascii string.
    /// </summary>
    /// <param name="reader">Source to read from</param>
    /// <param name="length">The length of the desired stream.</param>
    /// <returns>The parsed string.</returns>
    public static string ReadFixedAsciiString(this ref SpanParsingBuffer reader, int length)
    {
        var span = reader.Peek()[..length];
        var realLength = span.IndexOf((byte)0);
        if (realLength >= 0) span = span[..realLength];
        var ret = Encoding.ASCII.GetString(span);
        reader.Advance(length);
        return ret;
    }


    /// <summary>
    /// Read an array of unsigned 16 bit values values.
    /// </summary>
    /// <param name="reader">The reader to read from </param>
    /// <param name="len">Number of values to read</param>
    /// <returns>An array containing the read values.</returns>
    public static ushort[] ReadUshortArray(this ref SpanParsingBuffer reader, int len)
    {
        if (len == 0) return Array.Empty<ushort>();
        var ret = new ushort[len];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = reader.GetUInt16BigEndian();
        }
        return ret;
    }

    /// <summary>
    /// Reads an array of floats that are stored as uints scaled by a factor, which
    /// us stored first.  This format is unique to the ICC spec
    /// </summary>
    /// <param name="reader">Source of data to read from.</param>
    /// <param name="len">Length of the array to read</param>
    /// <param name="bytesPerSample">Number of bytes used to encode each element of the array.</param>
    /// <returns></returns>
    public static float[] ReadScaledFloatArray(
        this ref SpanParsingBuffer reader, int len, int bytesPerSample)
    {
        float divisor = (float)(1 << (8 * bytesPerSample)) - 1;
        var ret = new float[len];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = reader.ReadBigEndianUint(bytesPerSample)/divisor;
        }
        return ret;
    }

    /// <summary>
    /// Read a big endian UInt for the indicated number of bytes
    /// </summary>
    /// <param name="buffer">The buffer to read from</param>
    /// <param name="bytes">the number of bytes to read.</param>
    /// <returns></returns>
    public static UInt64 ReadBigEndianUint(this ref SpanParsingBuffer buffer, int bytes)
    {
        Debug.Assert(bytes is > 0 and <= 8);
        UInt64 ret = 0;
        for (int i = 0; i < bytes; i++)
        {
            ret <<= 8;
            ret |= buffer.GetUInt8();
        }
        return ret;
    }
    /// <summary>
    /// Read an array of floats in IEEE754 format.
    /// </summary>
    /// <param name="reader">A SequenceReader to read from.</param>
    /// <param name="len">Length of the array</param>
    /// <param name="spaceInFront">Number of zeros to leave on the front of the array.</param>
    /// <returns>The parsed array of floats</returns>
    public static float[] ReadIEEE754FloatArray(
        this ref SpanParsingBuffer reader, int len, int spaceInFront = 0)
    {
        var ret = new float[len + spaceInFront];
        for (int i = spaceInFront; i < ret.Length; i++)
        {
            ret[i] = ReadIEEE754Float(ref reader);
        }
        return ret;
    }

    /// <summary>
    /// Read a single IEEE754 formatted float from the reader.
    /// </summary>
    /// <param name="reader">Source of data to= parse</param>
    /// <returns>The float value returned from the SequenceReader.</returns>
    public static float ReadIEEE754Float(this ref SpanParsingBuffer reader) => 
        BitConverter.UInt32BitsToSingle(reader.GetUInt32BigEndian());

    /// <summary>
    /// Skup 4 bytes forward
    /// </summary>
    /// <param name="reader">The reader to skip forward.</param>
    public static void Skip32BitPad(ref this SpanParsingBuffer reader) => reader.Advance(4);

    /// <summary>
    /// Skip 2 bytes forward in the SequenceReader
    /// </summary>
    /// <param name="reader">The reader to skip forward</param>
    public static void Skip16BitPad(ref this SpanParsingBuffer reader) => reader.Advance(2);

    /// <summary>
    /// Skip 1 byte forward in the SequenceReader
    /// </summary>
    /// <param name="reader">The reader to skip forward</param>
    public static void Skip8BitPad(ref this SpanParsingBuffer reader) => reader.Advance(1);
}