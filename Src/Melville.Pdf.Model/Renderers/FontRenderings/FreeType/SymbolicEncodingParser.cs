using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using SharpFont;
using SharpFont.TrueType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class SymbolicEncodingParser
{
    public static async ValueTask<IGlyphMapping> ParseGlyphMapping(Face face, 
        PdfObject? encoding, PdfObject? type) =>
            type == KnownNames.Type1? 
                await MapType1Font(face, encoding).CA():
            UseTrueTypeFontMapping(face);

    private static async Task<SingleByteCharacterMapping> MapType1Font(Face face, PdfObject? encoding) =>
        GlyphMappingFactoy.FromFontFace(
            await RomanEncodingParser.InterpretEncodingValue(encoding, CharacterEncodings.Standard, 
                GlyphNamerFactory.CreateMapping(face)).CA(),
            face);

    private static IGlyphMapping UseTrueTypeFontMapping(Face face) =>
        new DictionaryGlyphMapping( 
             SelectCharMap(face)
                .AllMappings()
                .ToDictionary(i => (byte)i.Char, i => (char)i.Glyph));

    private static CharMap? SelectCharMap(Face face)
    {
        return face.CharMapByInts((PlatformId)3, 0) ?? face.CharMapByInts((PlatformId)1, 0);
    }
}

public static class FontFaceOperationss
{
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