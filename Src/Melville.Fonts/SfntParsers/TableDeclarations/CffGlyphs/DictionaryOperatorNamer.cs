namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

public static class DictionaryOperatorNamer
{
    public static string Title(int op) => $"{NameFor(op)} (0x{op:X})";
#if DEBUG
    private const int Shift = 0x0C00;
    private static string NameFor(int op) => op switch
    {
        // Top Dict Operators page 15
        0 => "Version",
        1 => "Notice",
        2 => "Full Name",
        3 => "Family Name",
        4 => "Weight",
        5 => "FontBBox",
        14 => "XUID",
        15 => "Charset",
        16 => "Encoding",
        17 => "CharStrings",
        18 => "Private",
        Shift + 0 => "Copyright",
        Shift + 1 => "isFixedPitch",
        Shift + 2 => "ItalicAngle",
        Shift + 3 => "UnderlinePosition",
        Shift + 4 => "UnderlineThickness",
        Shift + 5 => "PaintType",
        Shift + 6 => "CharstringType",
        Shift + 7 => "FontMatrix",
        Shift + 8 => "StrokeWidth",
        Shift + 20 => "SyntheticBase",
        Shift + 21 => "PostScript",
        Shift + 22 => "BaseFontName",
        Shift + 23 => "BaseFontBlend",
        Shift + 30 => "ROS",
        Shift + 31 => "CIDFontVersion",
        Shift + 32 => "CIDFontRevision",
        Shift + 33 => "CIDFontType",
        Shift + 34 => "CIDCount",
        Shift + 35 => "UIDBase",
        Shift + 36 => "FDArray",
        Shift + 37 => "FDSelect",
        Shift + 38 => "FontName",

        // cid top dict operators page 16

        // privaate dict operators page 24
        6 => "BlueValues",
        7 => "Other Blues",
        8 => "Family Blends",
        9 => "Family Other Blues",
        10 => "StdHW",
        11 => "StdVW",
        19 => "Subrs",
        20 => "DefaultWidthX",
        21 => "NominalWidthX",
        Shift + 9 => "BlueScale",
        Shift + 10 => "BlueShift",
        Shift + 11 => "BlueFuzz",
        Shift + 12 => "StemSnapH",
        Shift + 13 => "StemSnapV",
        Shift + 14 => "ForceBold",
        Shift + 17 => "LanguageGroup",
        Shift + 18 => "ExpansionFactor",
        Shift + 19 => "InitialRandomSeed",
        _ => $"Unknown Operator"
    };
#else
    private static string NameFor(int op) => $"Unknown Operator";
#endif
}