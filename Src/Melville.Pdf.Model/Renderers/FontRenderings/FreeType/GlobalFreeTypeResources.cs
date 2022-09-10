using System;
using System.IO;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GlobalFreeTypeResources
{
    public static readonly Library SharpFontLibrary = LoadFontLibrary();

    private static Library LoadFontLibrary()
    {
        var currentDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(Library).Assembly.Location)??
                                      throw new InvalidOperationException("Cannot find executable dir"));
        var ret = new Library();
        Directory.SetCurrentDirectory(currentDir);
        return ret;
    }

    private static FontLibrary? systemFontLibrary;
    public static FontLibrary SystemFontLibrary() =>
        systemFontLibrary ?? SetFontDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

    public static FontLibrary SetFontDirectory(string fontFolder) => 
        systemFontLibrary = new FontLibraryBuilder(SharpFontLibrary).BuildFrom(fontFolder);
}