using System;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GlobalFreeTypeResources
{
    public static readonly Library SharpFontLibrary = new Library();
    private static FontLibrary? systemFontLibrary;
    public static FontLibrary SystemFontLibrary() =>
        systemFontLibrary ?? SetFontDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

    public static FontLibrary SetFontDirectory(string fontFolder)
    {
        systemFontLibrary = new FontLibraryBuilder(GlobalFreeTypeResources.SharpFontLibrary).BuildFrom(fontFolder);
        return systemFontLibrary;
    }
}