namespace Melville.Pdf.LowLevel.Model.Conventions;

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

public static class HexMath
{

    public static Nibble ByteToNibble(byte digit) =>
        digit switch
        {
            >= (byte) '0' and <= (byte) '9' => (Nibble) (digit - (byte) '0'),
            >= (byte) 'A' and <= (byte) 'F' => (Nibble) (digit - ((byte) 'A' - 10)),
            >= (byte) 'a' and <= (byte) 'f' => (Nibble) (digit - ((byte) 'a' - 10)),
            _ => Nibble.Terminator
        };
    
    public static byte ByteFromHexCharPair(byte mostSig, byte leastSig) =>
        ByteFromNibbles(ByteToNibble(mostSig), ByteToNibble(leastSig));

    public static byte ByteFromNibbles(Nibble mostSig, Nibble leastSig) =>
        (mostSig, leastSig) switch
        {
            (_, Nibble.Terminator) => ByteFromNibbles(mostSig, 0),
            _ => (byte)( ((int)mostSig << 4) | (int)leastSig)

        };

    public static (byte MSB, byte LSB) CharPairFromByte(byte b) => 
        (HexDigits[b >> 4], HexDigits[b & 0xF]);

    public static byte[] HexDigits =
    {
        0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
        0x41, 0x42, 0x43, 0x44, 0x45, 0x46
    };
}