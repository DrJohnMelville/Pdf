using System.Collections.Generic;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly partial struct GlyphNameReader
{
    [FromConstructor] private readonly Face face;
    private readonly Dictionary<int, char> names = new();
    
    public  IGlyphNameMap FontNamings()
    {
        Debug.Assert(face.HasGlyphNames);
        ReadAllGlyphNames();
        return new GlyphNameToUnicodeMap(names);
    }
    
    private void ReadAllGlyphNames()
    {
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            ReadSingleGlyphName(face, i);
        }
    }

    private void ReadSingleGlyphName(Face face, uint i)
    {
        var nameKey = FnvHash.HashString(face.GetGlyphName(i, 30));
        names[(int)nameKey] = (char)i;
    }
}