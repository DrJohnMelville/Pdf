using System;
using System.Collections.Generic;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

internal class FontLibrary
{
    private IList<FontFamily> families;
    public FontLibrary(IList<FontFamily> families)
    {
        this.families = families;
    }

    public FontReference? FontFromName(in ReadOnlySpan<byte> fontName, bool bold, bool italic)
    {
        return 
            TrySuffix(fontName, "-Bold"u8, true, italic) ??
            TrySuffix(fontName, "-BoldItalic"u8, true, true) ??
            TrySuffix(fontName, "-Italic"u8, bold, true) ??
            TrySuffix(fontName, "-BoldOblique"u8, true, true) ??
            TrySuffix(fontName, "-Oblique"u8, bold, true) ??
            FindFontFamily(fontName)?.SelectFace(bold, italic);
    }

    public FontReference? TrySuffix(ReadOnlySpan<byte> fontName, ReadOnlySpan<byte> suffix, 
        bool bold, bool italic)
    {
        if (!fontName.EndsWith(suffix)) return null;
        return FontFromName(fontName[..^suffix.Length], bold, italic);
    }

    private FontFamily? FindFontFamily(in ReadOnlySpan<byte> fontName)
    {
        var familyNameString = Encoding.UTF8.GetString(fontName);
        FontFamily? ret = null;
        int currentLen = int.MaxValue;
        foreach (var family in families)
        {
            if (!StartsWith(family.FamilyName, fontName)) continue;
            if (family.FamilyName.Length > currentLen) continue;
            ret = family;
            currentLen = family.FamilyName.Length;
        }

        return ret;
    }

    private bool StartsWith(string familyName, in ReadOnlySpan<byte> fontName)
    {
        var familyIndex = 0;
        var fontIndex = 0;
        while (true)
        {
            SkipSpaces(familyName, ref familyIndex);
            SkipSpaces(fontName, ref fontIndex);
            if (fontIndex >= fontName.Length) return true;
            if (familyIndex >= familyName.Length) return false;
            if (familyName[familyIndex] != fontName[fontIndex]) return false;
            fontIndex++;
            familyIndex++;
        }
    }

    private static void SkipSpaces(string name, ref int index)
    {
        while (index < name.Length && name[index] == 32) index++;
    }
    private static void SkipSpaces(in ReadOnlySpan<byte> name, ref int index)
    {
        while (index < name.Length && name[index] == 32) index++;
    }
}