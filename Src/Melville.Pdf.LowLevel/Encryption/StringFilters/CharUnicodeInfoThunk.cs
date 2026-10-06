using System;
using System.Diagnostics;
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

internal static class CharUnicodeInfoThunk
{
    private static readonly BindingFlags bindingFlags =
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod;
    private static readonly MethodInfo method = ComputeMethodInfo();

    private static MethodInfo ComputeMethodInfo()
    {
        var typeCharUnicodeInfo = Type.GetType("System.Globalization.CharUnicodeInfo");

        var items = typeCharUnicodeInfo?.GetMethod("GetBidiCategoryNoBoundsChecks", bindingFlags);

        return items ??
               throw new InvalidOperationException("Cannot find CharUnicodeInfo");
    }

    internal const char HIGH_SURROGATE_START = '\ud800';
    internal const char HIGH_SURROGATE_END = '\udbff';
    internal const char LOW_SURROGATE_START = '\udc00';
    internal const char LOW_SURROGATE_END = '\udfff';
    internal const int HIGH_SURROGATE_RANGE = 0x3FF;

    internal const int UNICODE_CATEGORY_OFFSET = 0;
    internal const int BIDI_CATEGORY_OFFSET = 1;

    // The starting codepoint for Unicode plane 1.  Plane 1 contains 0x010000 ~ 0x01ffff.
    internal const int UNICODE_PLANE01_START = 0x10000;


    //This method was stolen from CharUnicodeInfo to get arround passing a span to a reflected method
    internal static CharacterKind GetBidiKind(string s, int index)
    {
        Debug.Assert(index >= 0 && index < s.Length, "index < s.Length");

        // The logic below follows Table 3-5 in the Unicode Standard, Sec. 3.9.
        // First char (high surrogate) = 110110wwwwxxxxxx
        // Second char (low surrogate) = 110111xxxxxxxxxx

        int c = s[index];
        if (index < s.Length - 1)
        {
            int temp1 = c - HIGH_SURROGATE_START; // temp1 = 000000wwwwxxxxxx
            if ((uint)temp1 <= HIGH_SURROGATE_RANGE)
            {
                int temp2 = s[index + 1] - LOW_SURROGATE_START; // temp2 = 000000xxxxxxxxxx
                if ((uint)temp2 <= HIGH_SURROGATE_RANGE)
                {
                    // |--------temp1--||-temp2--|
                    // 00000uuuuuuxxxxxxxxxxxxxxxx (where uuuuu = wwww + 1)
                    c = (temp1 << 10) + temp2 + UNICODE_PLANE01_START;
                }
            }
        }

        return (CharacterKind)method.Invoke(null, [(uint)c])!;
    }
}
