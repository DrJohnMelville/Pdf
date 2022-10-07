using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

public class FontLibrary
{
    private IList<FontFamily> families;
    public FontLibrary(IList<FontFamily> families)
    {
        this.families = families;
    }

    public FontReference? FontFromName(byte[] fontName, bool bold, bool italic)
    {
        var strName = fontName.ExtendedAsciiString();
        return FindFontFamily(fontName)?.SelectFace(bold, italic);
    }

    private FontFamily? FindFontFamily(byte[] fontName) =>
        families
            .Where(i => StartsWith(i.FamilyName, fontName))
            .MinBy(i => i.FamilyName.Length);

    private bool StartsWith(string familyName, byte[] fontName)
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
    private static void SkipSpaces(byte[] name, ref int index)
    {
        while (index < name.Length && name[index] == 32) index++;
    }
}