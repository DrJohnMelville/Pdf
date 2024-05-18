namespace Melville.Fonts.SfntParsers.TableDeclarations;

internal static class AsTagImplementation
{
    public static string AsTag(this UInt32 tagName) =>
        string.Create(4, tagName, TagCreator);

    private static void TagCreator(Span<char> span, uint arg)
    {
        span[0] = SingleChar(arg, 24);
        span[1] = SingleChar(arg, 16);
        span[2] = SingleChar(arg, 08);
        span[3] = SingleChar(arg, 0);
    }

    private static char SingleChar(uint arg, int bits) => (char)((arg >> bits) & 0xFF);
}