using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

public interface INameToGlyphMapping
{
    uint GetGlyphFor(byte[] name);
}

public readonly partial struct NameToGlyphMappingFactory
{
    [FromConstructor] private readonly Face face;

    public INameToGlyphMapping Create()
    {
        return new CompositeGlyphNameMapper(
            NamesFromFace(),
            UnicodeAndAdobeGlyphList(),
            UnicodeViaMacGlyphList()
        );
    }
    
    private INameToGlyphMapping? NamesFromFace()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(100);
        if (!face.HasGlyphNames) return null;
        var dictionary = new Dictionary<uint, uint>(face.GlyphCount);
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            dictionary[FnvHash.HashString(face.GetGlyphName(i, buffer))] = i;
        }
        ArrayPool<byte>.Shared.Return(buffer);
        return new DictionaryGlyphNameMapper(dictionary);
    }

    private INameToGlyphMapping? UnicodeAndAdobeGlyphList()
    {
        var mapping = face.CharMaps.FirstOrDefault(i => i.Encoding == Encoding.Unicode);
        if (mapping is null) return null;
        return new UnicodeGlyphNameMapper(MappingToDictionary(mapping));
    }

    private static Dictionary<uint, uint> MappingToDictionary(CharMap mapping) => 
        mapping.AllMappings().ToDictionary(i => i.Char, i => i.Glyph);

    private INameToGlyphMapping UnicodeViaMacGlyphList()
    {
        var mapping = face.CharMapByInts(1, 0);
        if (mapping is null) return null;
        return new UnicodeGlyphNameMapper(MappingToDictionary(mapping));
    }

}