using System;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

public static class Modulo3Computer
{
    public static int Mod3(this in Span<byte> bytes) => Mod3((ReadOnlySpan<byte>)bytes);
    /// <summary>
    /// Interpret the span as a big endian large number and compute the modulo 3 using
    /// the algorithm at: http://homepage.cs.uiowa.edu/~dwjones/bcd/mod.shtml#exmod3
    /// </summary>
    /// <param name="bytes">A span of bytes interpreted as a big endian large integer</param>
    /// <returns>0, 1, or 2 -- representing the modulo 3 of the big integer</returns>
    public static int Mod3(this in ReadOnlySpan<byte> bytes)
    {
        int accum = 0;
        foreach (var item in bytes)
        {
            for (byte partialByte = item; partialByte > 0; partialByte >>= 2)
            {
                accum += partialByte & 0x03;
            }
        }

        return accum % 3;
    }
}