using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

public class FontLibrary
{
    private IList<FontFamily> families;
    public FontLibrary(IList<FontFamily> families)
    {
        this.families = families;
    }

    public FontReference FontFromName(byte[] fontName, bool bold, bool italic)
    {
        return FindFontFamily(fontName).SelectFace(bold, italic);
    }

    private FontFamily FindFontFamily(byte[] fontName)
    {
        int currentLen = -1;
        FontFamily? result = null;
        foreach (var family in families)
        {
            var len = fontName.CommonPrefixLength(family.FamilyName);
            if (len > currentLen || 
                (len == currentLen && family.FamilyName.Length < (result?.FamilyName.Length??1000)))
            {
                currentLen = len;
                result = family;
            }
        }
        return result ?? families.First();
    }

}