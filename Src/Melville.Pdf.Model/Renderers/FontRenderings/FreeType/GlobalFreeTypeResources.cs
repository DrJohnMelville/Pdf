using System;
using System.IO;
using Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GlobalFreeTypeResources
{
    public static readonly Library SharpFontLibrary = LoadFontLibrary();

    private static Library LoadFontLibrary()
    {
        var currentDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetDirectoryName(AssemblyPath())??
                                      throw new InvalidOperationException("Cannot find executable dir"));
        Console.WriteLine($"Current Dir: {Directory.GetCurrentDirectory()}");
        var ret = new Library();
        Directory.SetCurrentDirectory(currentDir);
        return ret;
    }

    private static string AssemblyPath()
    {
        return typeof(Library).Assembly.Location;
    }

    public static T Debug<T>(T input)
    {
        Console.WriteLine(input);
        return input;
    }
}