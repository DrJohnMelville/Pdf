using System;

namespace Melville.Postscript.Interpreter.Values.Strings;

internal static class SevenBitStringEncoding
{
    const int BitsPerCharacter = 7;
    const int OneCharacterMask = (1 << BitsPerCharacter) - 1;

    public static byte GetNextCharacter(ref Int128 remainingChars)
    {
        var ret = (byte)(OneCharacterMask & remainingChars);
        remainingChars >>>= BitsPerCharacter;
        return ret;
    }

    public static void AddOneCharacter(ref Int128 partialString, byte character)
    {
        partialString <<= BitsPerCharacter;
        partialString |= character & OneCharacterMask;
    }
}