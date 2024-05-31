namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs;

[Flags]
internal enum CompositeGlyphFlags : ushort
{
    Arg1And2AreWords = 0x0001,
    ArgsAreXYValues = 0x0002,
    RoundXYToGrid = 0x0004,
    WeHaveAScale = 0x0008,
    MoreComponents = 0x0020,
    WeHaveAnXAndYScale = 0x0040,
    WeHaveATwoByTwo = 0x0080,
    WeHaveInstructions = 0x0100,
    UseMyMetrics = 0x0200,
    OverlapCompound = 0x0400,
    ScaledComponentOffset = 0x0800,
    UnscaledComponentOffset = 0x1000,
}

internal static class CompostieGlyphFlagCheckers
{
    public static bool ArgsAreWords(this CompositeGlyphFlags f) =>
        (f & CompositeGlyphFlags.Arg1And2AreWords) != 0;
    public static bool ArgsAreXYOffsets(this CompositeGlyphFlags f) =>
        (f & CompositeGlyphFlags.ArgsAreXYValues) != 0;
    public static bool HasMoreGlyphs(this CompositeGlyphFlags f) =>
        (f & CompositeGlyphFlags.MoreComponents) != 0;

    public static CompositeGlyphFlags ScaleSelector(this CompositeGlyphFlags f) =>
        f & (CompositeGlyphFlags.WeHaveAScale | 
             CompositeGlyphFlags.WeHaveAnXAndYScale |
             CompositeGlyphFlags.WeHaveATwoByTwo);

    public static bool ShouldScaleOffset(this CompositeGlyphFlags f) =>
        (f & (CompositeGlyphFlags.ScaledComponentOffset |
                 CompositeGlyphFlags.UnscaledComponentOffset)) ==
            CompositeGlyphFlags.ScaledComponentOffset;
}