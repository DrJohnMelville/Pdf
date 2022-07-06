using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class SymbolicEncodingParser
{
    public static async ValueTask<IGlyphMapping> ParseGlyphMapping(Face face, PdfObject? encoding)
    {
        TrySelectAppleRomanCharMap(face);
        return new UnicodeGlyphMapping(face, 
            await RomanEncodingParser.InterpretEncodingValue(encoding, new PassthroughMapping()).CA());
    }

    private static void TrySelectAppleRomanCharMap(Face face)
    {
        if (face.CharMaps.FirstOrDefault(i=>i.Encoding == Encoding.AppleRoman) is {} charMap)
            face.SetCharmap(charMap);
    }
}