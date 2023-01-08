using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.SharpFont;
using Melville.SharpFont.TrueType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class FontFaceOperations
{
    public static CharMap? CharMapByInts(this Face face, int platformId, int encodingId) =>
        face.CharMapByInts((PlatformId)platformId, encodingId);
    public static CharMap? CharMapByInts(this Face face, PlatformId platformId, int encodingId) =>
        face.CharMaps?.FirstOrDefault(i => i.PlatformId == platformId && i.EncodingId == encodingId);

    public static IEnumerable<(uint Char, uint Glyph)> AllMappings(this CharMap cMap)
    {
        if (TrySetCharMap(cMap)) yield break;
        uint character = cMap.Face.GetFirstChar(out var glyph);
        do
        {
            yield return (character, glyph);
            character = cMap.Face.GetNextChar(character, out glyph);
        } while (character != 0);
    }

    private static bool TrySetCharMap(CharMap cMap)
    {
        try
        {
            cMap.Face.SetCharmap(cMap);
        }
        catch (FreeTypeException)
        {
            return true;
        }

        return false;
    }

    public static IEnumerable<(uint Glyph, string Name)> AllGlyphNames(this Face face)
    {
        if (!face.HasGlyphNames) yield break;
        var buffer = ArrayPool<byte>.Shared.Rent(100);
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            yield return (i, face.GetGlyphName(i, buffer));
        }
        ArrayPool<byte>.Shared.Return(buffer);
    }
}