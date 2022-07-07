using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public  class FreeTypeGlyphNameMap : IGlyphNameMap
{
    private readonly Dictionary<PdfName, char> glyphNameKeys = new();
    public FreeTypeGlyphNameMap(Face face)
    {
        ReadAllGlyphNames(face);
    }

    private void ReadAllGlyphNames(Face face)
    {
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            ReadSingleGlyphName(face, i);
        }
    }

    private void ReadSingleGlyphName(Face face, uint i)
    {
        var nameKey = NameDirectory.Get(face.GetGlyphName(i, 30));
        glyphNameKeys[nameKey] = (char)i;
    }

    public char Map(PdfName input) => 
        glyphNameKeys.TryGetValue(input, out var glyph) ? glyph : '\0';
}