using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal readonly partial struct NameToGlyphMappingFactory
{
    [FromConstructor] private readonly Face face;

    public INameToGlyphMapping Create() =>
        new CompositeGlyphNameMapper(
            NamesFromFace(),
            UnicodeAndAdobeGlyphList(),
            UnicodeViaMacGlyphList()
        );

    private INameToGlyphMapping? NamesFromFace()
    {
        if (!face.HasGlyphNames) return null;
        var dictionary = new Dictionary<uint, uint>(face.GlyphCount);
        foreach (var (glyph, name) in face.AllGlyphNames())
        {
            dictionary[FnvHash.FnvHashAsUInt(name)] = glyph;
        }

        return new DictionaryGlyphNameMapper(dictionary);
    }

    private INameToGlyphMapping? UnicodeAndAdobeGlyphList()
    {
        var mapping = face.CharMaps?.FirstOrDefault(i => i.Encoding == Encoding.Unicode);
        if (mapping is null) return null;
        return new UnicodeGlyphNameMapper(MappingToDictionary(mapping));
    }

    private static Dictionary<uint, uint> MappingToDictionary(CharMap mapping) => 
        mapping.AllMappings().ToDictionary(i => i.Char, i => i.Glyph);

    private INameToGlyphMapping? UnicodeViaMacGlyphList()
    {
        var mapping = face.CharMapByInts(1, 0);
        if (mapping is null) return null;
        return new UnicodeGlyphNameMapper(MappingToDictionary(mapping));
    }

}