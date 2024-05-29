namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

[Flags]
public enum GlyphFlags : byte
{
    OnCurve = 0x01,
    XShortVector = 0x02,
    YShortVector = 0x04,
    Repeat = 0x08,
    XIsSame = 0x10,
    YIsSame = 0x20,
    Overlapping = 0x40,
    Reserved = 0x80
}

public static class GlyphFlagOperations
{
    public static bool Check(this GlyphFlags flags, GlyphFlags check) =>
        (flags & check) != 0;

    public static int ComputePointSizeAndSign(this GlyphFlags flag, GlyphFlags shortFlag, GlyphFlags isSameFlag) =>
        (flag.Check(shortFlag), flag.Check(isSameFlag)) switch
        {
            (false, true) => 0,
            (false, false) => 2,
            (true, true) => 1,
            (true, false) => -1,
        };
}