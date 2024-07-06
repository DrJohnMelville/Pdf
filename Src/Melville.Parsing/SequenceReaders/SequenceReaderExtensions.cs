using System.Buffers;
using System.Numerics;
using Melville.INPC;

namespace Melville.Parsing.SequenceReaders;

/// <summary> Extension methods to read a variety of binary number formats from a SequenceReader.
/// </summary>
[MacroItem("byte", "Uint8", 1)]
[MacroItem("sbyte", "Int8", 1)]
[MacroItem("short", "Int16", 2)]
[MacroItem("ushort", "Uint16", 2)]
[MacroItem("int", "Int32", 4)]
[MacroItem("uint", "Uint32", 4)]
[MacroItem("long", "Int64", 8)]
[MacroItem("ulong", "Uint64", 8)]
[MacroCode("""
        /// <summary>
        /// Try to read a bug endian value from the reader
        /// </summary>
        /// <param name="reader">Source of data to parse from</param>
        /// <returns>The number read from the reader</returns>
        /// <exception cref="InvalidDataException">If there is not enough bytes in the reader.</exception>
        public static ~0~ ReadBigEndian~1~(this ref SequenceReader<byte> reader) => (~0~)reader.ReadBigEndianUint(~2~);
        
        """)]
[MacroCode("""
        /// <summary>
        /// Attempts to read a big endian value of a given number of bytes into a ulong. 
        /// </summary>
        /// <param name="reader">The source of bytes for the data.</param>
        /// <param name="ret">Out parameter for the result of the parsing operation</param>
        /// <returns>True if there are enough bytes to read.  False otherwise.</returns>
        public static bool TryReadBigEndian~1~(this ref SequenceReader<byte> reader, out ~0~ ret) 
        {
            return reader.TryReadBigEndian(out ret);
        }

        """)]


public static  partial class SequenceReaderExtensions
{
    /// <summary>
    /// Read any Binary Integer type in big endian format
    /// </summary>
    /// <typeparam name="T">The type of number to read</typeparam>
    /// <param name="reader">The sequence reader to take bytes from.</param>
    /// <param name="value">Out variable that receives the output</param>
    /// <returns>True if the read is successdful, false if there is not enough data.</returns>
    public static bool TryReadBigEndian<T>(this ref SequenceReader<byte> reader, out T value)
        where T : IBinaryInteger<T> =>
        GenericIntReader<T>.TryReadBigEndian(ref reader, out value);
    
    /// <summary>
    /// Attempts to read a big endian value of a given number of bytes into a ulong. 
    /// </summary>
    /// <param name="reader">The source of bytes for the data.</param>
    /// <param name="value">Out parameter for the result of the parsing operation</param>
    /// <param name="byteCount">Number of bytes to read</param>
    /// <returns>True if there are enough bytes to read.  False otherwise.</returns>
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

    /// <summary>
    /// Try to read a bug endian value from the reader
    /// </summary>
    /// <param name="reader">Source of data to parse from</param>
    /// <param name="bytes">number of bytes to read.</param>
    /// <returns>The number read from the reader</returns>
    /// <exception cref="InvalidDataException">If there is not enough bytes in the reader.</exception>
    public static ulong ReadBigEndianUint(this ref SequenceReader<byte> reader, int bytes)
    {
        if (!reader.TryReadBigEndian(out var ret, bytes))
            throw new InvalidDataException("Not enough input");
        return ret;
    }

    /// <summary>
    /// Reads a float encoded using the fixed 15/16 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Reads15Fixed16(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianInt16()) + (((float)source.ReadBigEndianUint16()) / ((1 << 16) - 1));

    /// <summary>
    /// Reads a positive float encoded using the fixed 16/16 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Readu16Fixed16(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianUint16()) + (((float)source.ReadBigEndianUint16()) / ((1 << 16) - 1));

    /// <summary>
    /// Reads a float encoded using the fixed 8/8 bit format from the ICC spec
    /// </summary>
    /// <param name="source">Source to read from</param>
    /// <returns>The parsed number</returns>
    public static float Readu8Fixed8(this ref SequenceReader<byte> source) =>
        ((float)source.ReadBigEndianUint8()) + (((float)source.ReadBigEndianUint8()) / ((1 << 8) - 1));
    
    /// <summary>
    /// Read a sequence of bytes as an ascii string.
    /// </summary>
    /// <param name="reader">Source to read from</param>
    /// <param name="length">The length of the desired stream.</param>
    /// <returns>The parsed string.</returns>
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

    /// <summary>
    /// Read an array of unsigned 16 bit values values.
    /// </summary>
    /// <param name="reader">The reader to read from </param>
    /// <param name="len">Number of values to read</param>
    /// <returns>An array containing the read values.</returns>
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

    /// <summary>
    /// Reads an array of floats that are stored as uints scaled by a factor, which
    /// us stored first.  This format is unique to the ICC spec
    /// </summary>
    /// <param name="reader">Source of data to read from.</param>
    /// <param name="len">Length of the array to read</param>
    /// <param name="bytesPerSample">Number of bytes used to encode each element of the array.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Read an array of floats in IEEE754 format.
    /// </summary>
    /// <param name="reader">A SequenceReader to read from.</param>
    /// <param name="len">Length of the array</param>
    /// <param name="spaceInFront">Number of zeros to leave on the front of the array.</param>
    /// <returns>The parsed array of floats</returns>
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

    /// <summary>
    /// Read a single IEEE754 formatted float from the reader.
    /// </summary>
    /// <param name="reader">Source of data to= parse</param>
    /// <returns>The float value returned from the SequenceReader.</returns>
    public static float ReadIEEE754Float(this ref SequenceReader<byte> reader) => 
        BitConverter.UInt32BitsToSingle(reader.ReadBigEndianUint32());

    /// <summary>
    /// Skip 4 bytes forward in the SequenceReader
    /// </summary>
    /// <param name="reader">The reader to skip forward</param>
    public static void Skip32BitPad(ref this SequenceReader<byte> reader) => reader.Advance(4);

    /// <summary>
    /// Skip 2 bytes forward in the SequenceReader
    /// </summary>
    /// <param name="reader">The reader to skip forward</param>
    public static void Skip16BitPad(ref this SequenceReader<byte> reader) => reader.Advance(2);

    /// <summary>
    /// Skip 1 byte forward in the SequenceReader
    /// </summary>
    /// <param name="reader">The reader to skip forward</param>
    public static void Skip8BitPad(ref this SequenceReader<byte> reader) => reader.Advance(1);


    /// <summary>
    /// Try to advance a sequence reader by a given number of positions.
    /// </summary>
    /// <typeparam name="T">The element type of the SequenceReader</typeparam>
    /// <param name="input">The sequenceReader to advance.</param>
    /// <param name="positions"></param>
    /// <returns>True if the advance succeeded, or false if not enough elementns remain</returns>
    public static bool TryAdvance<T>(this ref SequenceReader<T> input, int positions) 
        where T:unmanaged, IEquatable<T>
    {
        if (input.Remaining < positions) return false;
        input.Advance(positions);
        return true;
    }
}