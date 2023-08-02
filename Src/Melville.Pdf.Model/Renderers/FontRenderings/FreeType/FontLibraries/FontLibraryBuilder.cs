using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

internal readonly struct FontLibraryBuilder
{
    private readonly Library sharpFontLibrary;
    private readonly Dictionary<string, FontFamily> fonts = new( );

    public FontLibraryBuilder()
    {
        sharpFontLibrary = GlobalFreeTypeResources.SharpFontLibrary;
    }

    public FontLibrary BuildFrom(string fontFolder)
    {
        RegisterAllFonts(fontFolder);
        return new FontLibrary(fonts.Values.ToArray());
    }

    private void RegisterAllFonts(string fontFolder)
    {
        foreach (var fontFile in Directory.EnumerateFiles(fontFolder)) RegisterFont(fontFile);
    }

    private void RegisterFont(string fileName)
    {
        int totalFaces = 1;
        for (int i= 0; i < totalFaces; i++)
        {
            totalFaces = TryRegisterSingleFace(fileName, i);
        }
    }

    private int TryRegisterSingleFace(string fileName, int index)
    {
        try
        {
            //SharpFont cannot read all font files but the only way to know is to try
            // so we just try everything and ignore the files we cannot read.
            return RegisterSingleFace(fileName, index);
        }
        catch (FreeTypeException)
        {
            return 0;
        }
    }

    private int RegisterSingleFace(string fileName, int index)
    {
        using var face = sharpFontLibrary.NewFace(fileName, index);
        //SharpFont is not attributed for nullable reference types, but the documentation
        // indicates that face.FamilyName can be null;
        RegisterFaceWithStyle(new FontReference(fileName, index), face.FamilyName, face.StyleFlags);
        return face.FaceCount;
    }

    private void RegisterFaceWithStyle(FontReference fontReference, string? family, StyleFlags style) => 
        GetOrCreateFamily(family??"Unnamed").Register(
            fontReference, style.HasFlag(StyleFlags.Bold), style.HasFlag(StyleFlags.Italic));

    private FontFamily GetOrCreateFamily(string family)
    {
        if (!fonts.TryGetValue(family, out var found))
        {
            found = new FontFamily(family);
            fonts.Add(family, found);
        }
        return found;
    }
}