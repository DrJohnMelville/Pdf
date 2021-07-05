namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
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
    }
}