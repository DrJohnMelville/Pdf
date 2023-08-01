using System;
using System.Globalization;
using System.Reflection;

namespace Melville.Pdf.LowLevel.Encryption.StringFilters;

[Flags]
internal enum CharacterKind
{
    None = 0b0000,
    RorAL = 0x40,
    LCat = 0x20,
    Error = 0x01
}

internal static class SaslValidator
{
    public static bool IsValid(string input)
    {
        if (input.Length == 0) return true;
        var allClasses = CharacterKind.None;
        for (int i = 0; i < input.Length; i++)
        {
            allClasses |= KindOfCharacter(input, i);
        }

        return (!allClasses.HasFlag(CharacterKind.Error)) &&
               IsValidBidi(allClasses, input);
    }

    private static bool IsValidBidi(CharacterKind allClasses, string input)
    {
        if (!allClasses.HasFlag(CharacterKind.RorAL)) return true;
        if (allClasses.HasFlag(CharacterKind.LCat)) return false;
        return (KindOfCharacter(input, 0) is CharacterKind.RorAL &&
                KindOfCharacter(input, input.Length - 1) is CharacterKind.RorAL);
    }

    private static CharacterKind KindOfCharacter(string s, int position) =>
        (IsForbiddenChar(s[position]) ? CharacterKind.Error : CharacterKind.None) |
        CharUnicodeInfoThunk.GetBidiKind(s, position);

    private static bool IsForbiddenChar(char c) => c is
        // NonAscii space characters
        '\xA0' or '\x1680' or (>= '\x2000' and <= '\x200A') or '\x202F' or '\x205F' or '\x3000' or
        // ascii control characters
        <= '\x001F' or '\x007F' or
        // non-ascii control characters
        (>= '\x0080' and <= '\x009F') or
        '\x06DD' or
        '\x070F' or '\x180E' or '\x200C' or '\x200D' or '\x2028' or '\x2029' or '\x2060' or
        '\x2061' or '\x2062' or '\x2063' or (>= '\x206A' and <= '\x206F') or '\xFEFF' or
        (>= '\xFFF9' and <= '\xFFFC') or
        // private use characters
        (>= '\xE000' and <= '\xF8FF') or
        // NonCharacter code points
        (>= '\xFDD0' and <= '\xFDEF') or
        (>= '\xFFFE' and <= '\xFFFF') or
        // surrogate codes
        (>= '\xD800' and <= '\xDFFF') or
        // inappropriate for plain text
        '\xFFF9' or '\xFFFA' or '\xFFFB' or '\xFFFC' or '\xFFFD' or
        // inappropriate for cannonical representation
        (>= '\x2FF0' and <= '\x2FFB') or
        // Change Display properties or are depreciated.
        '\x0340' or '\x0341' or
        '\x200E' or '\x200F' or '\x202A' or '\x202B' or '\x202C' or '\x202D' or
        '\x202E' or '\x206A' or '\x206B' or '\x206C' or '\x206D' or '\x206E'
        or '\x206F';
}

internal static class CharUnicodeInfoThunk
{
    private static readonly BindingFlags bindingFlags = 
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod;
    private static readonly MethodInfo method = ComputeMethodInfo();

    private static MethodInfo ComputeMethodInfo()
    {
        var typeCharUnicodeInfo = Type.GetType("System.Globalization.CharUnicodeInfo");
        return typeCharUnicodeInfo?.GetMethod("GetBidiCategory", bindingFlags, null,
                   CallingConventions.Any, new Type[]{typeof(string), typeof(int)}, null)??
               throw new InvalidOperationException("Cannot find CharUnicodeInfo");
    }

    internal static CharacterKind GetBidiKind(string s, int position)
    {
        var parameters = new object[] { s, position };
        return (CharacterKind)
            method.Invoke(null, bindingFlags, null, parameters, CultureInfo.InvariantCulture)!;
    }
}
