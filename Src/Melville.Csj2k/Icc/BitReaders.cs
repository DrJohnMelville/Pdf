using Melville.CSJ2K.Icc.Types;

namespace Melville.CSJ2K.Icc;

public static class BitReaders
{
    /// <summary> Create an XYZNumber from byte [] input</summary>
    /// <param name="data">array containing the XYZNumber representation
    /// </param>
    /// <param name="offset">start of the rep in the array
    /// </param>
    /// <returns> the created XYZNumber
    /// </returns>
    public static XYZNumber getXYZNumber(byte[] data, int offset)
    {
        int x, y, z;
        x = getInt(data, offset);
        y = getInt(data, offset + int_size);
        z = getInt(data, offset + 2 * int_size);
        return new XYZNumber(x, y, z);
    }

    /// <summary> Create an ICCDateTime from byte [] input</summary>
    /// <param name="data">array containing the ICCProfileVersion representation
    /// </param>
    /// <param name="offset">start of the rep in the array
    /// </param>
    /// <returns> the created ICCProfileVersion
    /// </returns>
    public static ICCDateTime getICCDateTime(byte[] data, int offset)
    {
        short wYear = getShort(data, offset); // Number of the actual year (i.e. 1994)
        short wMonth = getShort(data, offset + short_size); // Number of the month (1-12)
        short wDay = getShort(data, offset + 2 * short_size); // Number of the day
        short wHours = getShort(data, offset + 3 * short_size); // Number of hours (0-23)
        short wMinutes = getShort(data, offset + 4 * short_size); // Number of minutes (0-59)
        short wSeconds = getShort(data, offset + 5 * short_size); // Number of seconds (0-59)
        return new ICCDateTime(wYear, wMonth, wDay, wHours, wMinutes, wSeconds);
    }

    /// <summary>Size of native type </summary>
    public const int byte_size = 1;

    /// <summary>Size of native type </summary>
    public const int short_size = 2;

    /// <summary>Size of native type </summary>
    public const int int_size = 4;

    public const int BITS_PER_BYTE = 8;
    public const int BITS_PER_SHORT = 16;
    public const int BITS_PER_INT = 32;
    public const int BYTES_PER_INT = 4;
    public const int BYTES_PER_LONG = 8;

    /// <summary> Create a short from a two byte [].</summary>
    /// <param name="bfr">data array
    /// </param>
    /// <param name="off">start of data in array
    /// </param>
    /// <returns> native type from representation.
    /// </returns>
    public static short getShort(byte[] bfr, int off)
    {
        int tmp0 = bfr[off] & 0xff; // Clear the sign extended bits in the int.
        int tmp1 = bfr[off + 1] & 0xff;
        return (short)(tmp0 << BITS_PER_BYTE | tmp1);
    }

    /// <summary> Separate bytes in a long into a byte array lsb to msb order.</summary>
    /// <param name="d">long to separate
    /// </param>
    /// <returns> byte [] containing separated int.
    /// </returns>
    public static byte[] setLong(long d)
    {
        return setLong(d, new byte[BYTES_PER_INT]);
    }

    /// <summary> Separate bytes in a long into a byte array lsb to msb order.
    /// Return the result in the provided array
    /// </summary>
    /// <param name="d">long to separate
    /// </param>
    /// <param name="b">return output here.
    /// </param>
    /// <returns> reference to output.
    /// </returns>
    public static byte[] setLong(long d, byte[] b)
    {
        if (b == null) b = new byte[BYTES_PER_LONG];
        for (int i = 0; i < BYTES_PER_LONG; ++i)
        {
            b[i] = (byte)(d & 0x0ff);
            d = d >> BITS_PER_BYTE;
        }
        return b;
    }

    /// <summary> Create an int from a byte [4].</summary>
    /// <param name="bfr">data array
    /// </param>
    /// <param name="off">start of data in array
    /// </param>
    /// <returns> native type from representation.
    /// </returns>
    public static int getInt(byte[] bfr, int off)
    {

        int tmp0 = getShort(bfr, off) & 0xffff; // Clear the sign extended bits in the int.
        int tmp1 = getShort(bfr, off + 2) & 0xffff;

        return tmp0 << BITS_PER_SHORT | tmp1;
    }

    /// <summary> Create an long from a byte [8].</summary>
    /// <param name="bfr">data array
    /// </param>
    /// <param name="off">start of data in array
    /// </param>
    /// <returns> native type from representation.
    /// </returns>
    public static long getLong(byte[] bfr, int off)
    {

        long tmp0 = getInt(bfr, off) & unchecked((int)0xffffffff); // Clear the sign extended bits in the int.
        long tmp1 = getInt(bfr, off + 4) & unchecked((int)0xffffffff);

        return tmp0 << BITS_PER_INT | tmp1;
    }
}