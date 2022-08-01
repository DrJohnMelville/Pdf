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

    private static async Task<UnicodeGlyphMapping> MapType1Font(Face face, PdfObject? encoding)
    {
        return new UnicodeGlyphMapping(face,
            await RomanEncodingParser.InterpretEncodingValue(encoding, CharacterEncodings.Standard).CA());
    }

    private static IGlyphMapping UseTrueTypeFontMapping(Face face)
    {
        var cmap = face.CharMapByInts((PlatformId)3, 0) ?? face.CharMapByInts((PlatformId)1, 0);

        // need to handle the correct plates-+
        Dictionary<byte, char> mapping = new();
        if (cmap is not null)
        {
            WriteCharMapToDictionary(cmap, mapping);
        }
        return new DictionaryGlyphMapping( mapping);
    }

    private static void WriteCharMapToDictionary(CharMap cmap, Dictionary<byte, char> mapping)
    {
        cmap.Face.SelectCharmap(cmap.Encoding);
        var index = cmap.Face.GetFirstChar(out var glyph);
        while (index != 0)
        {
            mapping[(byte)(index & 0xff)] = (char)glyph;
            index = cmap.Face.GetNextChar(index, out glyph);
        }
    }
}

public static class FontFaceOperationss
{
    public static CharMap? CharMapByInts(this Face face, PlatformId platformId, int encodingId) =>
        face.CharMaps.FirstOrDefault(i => i.PlatformId == platformId && i.EncodingId == encodingId);
}