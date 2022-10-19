using System.Text;

namespace Melville.Pdf.LowLevel.Encryption.StringFilters;

public static class SaslCharacterMapping
{
    public static string MapChars(string input)
    {
        return IsNormalized(input) ? input : InnerNormalize(input);
    }


    private static bool IsNormalized(string s)
    {
        foreach (var character in s)
        {
            if (Map(character) != character) return false;
        }

        return true;
    }
    private static string InnerNormalize(string input)
    {
        var builder = new StringBuilder(input.Length);
        foreach (var character in input)
        {
            var mapped = Map(character);
            if (mapped != SpecialChars.NotACharacer)
                builder.Append(mapped);
        }

        return builder.ToString();
    }

    private static char Map(char character) => character switch
    {
        '\xA0' or '\x1680' or (>= '\x2000' and <= '\x200A') or '\x202F' or '\x205F' or
        '\x3000' => ' ',
        '\x00AD' or
    '\x034F' or
    '\x1806' or
    '\x180B' or
    '\x180C' or
    '\x180D' or
    (>= '\x200B'  and <='\x200D') or
    '\x2060' or
    (>= '\xFE00' and <= '\xFE0F') or
        '\xFEFF' => SpecialChars.NotACharacer,
        _ => character
    };
}