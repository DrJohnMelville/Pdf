using System.Collections.Generic;
using System.Linq;
using SharpFont;
using SharpFont.TrueType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class FontFaceOperations
{
    public static CharMap? CharMapByInts(this Face face, int platformId, int encodingId) =>
        face.CharMapByInts((PlatformId)platformId, encodingId);
    public static CharMap? CharMapByInts(this Face face, PlatformId platformId, int encodingId) =>
        face.CharMaps.FirstOrDefault(i => i.PlatformId == platformId && i.EncodingId == encodingId);

    public static IEnumerable<(uint Char, uint Glyph)> AllMappings(this CharMap cMap)
    {
        cMap.Face.SetCharmap(cMap);
        for (uint character = cMap.Face.GetFirstChar(out var glyph);
             character != 0;
             character = cMap.Face.GetNextChar(character, out glyph))
        {
            yield return (character, glyph);
        }
    }
}