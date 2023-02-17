using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal static class GlobalFreeTypeResources
{
    private static Library? sharpFontLibrary = null;
    internal static Library SharpFontLibrary => sharpFontLibrary ??= LoadFontLibrary();

    public static Library LoadFontLibrary(string? freetype6DllFolder = null) => new(freetype6DllFolder);
}