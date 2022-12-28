using System;

namespace Melville.Pdf.LowLevel.Model.Conventions;

/// <summary>
/// This enum represents a nibble or 4 bit value in the HexMath routines.
/// </summary>
public enum Nibble 
{
    N0 = 0,
    N1,
    N2,
    N3,
    N4,
    N5,
    N6,
    N7,
    N8,
    N9,
    N10,
    N11,
    N12,
    N13,
    N14,
    N15,
    OutOfSpace,
    Terminator = 255
}

/// <summary>
/// This is a small utility class arround hexadecimal conversions
/// </summary>
public static class HexMath
{

    /// <summary>
    /// Convert a single hex digit (as a character) to a nibble.
    /// </summary>
    /// <param name="digit">the digit to convert</param>
    /// <returns>A nibble representing the value of the digit, or Nibble.Terminator if the character is not a nibble.</returns>
    public static Nibble ByteToNibble(byte digit) =>
        digit switch
        {
            >= (byte) '0' and <= (byte) '9' => (Nibble) (digit - (byte) '0'),
            >= (byte) 'A' and <= (byte) 'F' => (Nibble) (digit - ((byte) 'A' - 10)),
            >= (byte) 'a' and <= (byte) 'f' => (Nibble) (digit - ((byte) 'a' - 10)),
            _ => Nibble.Terminator
        };
    
    /// <summary>
    /// Get a byte from a pair of two characters
    /// </summary>
    /// <param name="mostSig">The most significant character</param>
    /// <param name="leastSig">The least significant character</param>
    /// <returns></returns>
    public static byte ByteFromHexCharPair(byte mostSig, byte leastSig) =>
        ByteFromNibbles(ByteToNibble(mostSig), ByteToNibble(leastSig));

    /// <summary>
    /// Compute a byte from two nibbles
    /// </summary>
    /// <param name="mostSig"></param>
    /// <param name="leastSig"></param>
    /// <returns></returns>
    public static byte ByteFromNibbles(Nibble mostSig, Nibble leastSig) =>
        (mostSig, leastSig) switch
        {
            (_, Nibble.Terminator) => ByteFromNibbles(mostSig, 0),
            _ => (byte)( ((int)mostSig << 4) | (int)leastSig)

        };

    /// <summary>
    /// Given a byte, compute the two character hexadecimal code.
    /// </summary>
    /// <param name="b">The byte to convert</param>
    /// <returns>A pair of bytes representing the characters for the hex representation</returns>
    public static (byte MSB, byte LSB) CharPairFromByte(byte b) => 
        (HexDigits[b >> 4], HexDigits[b & 0xF]);

    /// <summary>
    /// This span contains the hex digits 0-F such that HexDigits[x] == the hex digit for x;
    /// </summary>
    public static ReadOnlySpan<byte> HexDigits => "0123456789ABCDEF"u8;
}