using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class SymbolicEncodingParser
{
    public static ValueTask<IGlyphMapping> ParseGlyphMapping(Face face, PdfObject? encoding)
    {
        TrySelectAppleRomanCharMap(face);
        return new (new UnicodeGlyphMapping(face, new PassthroughMapping()));
    }

    private static void TrySelectAppleRomanCharMap(Face face)
    {
        if (face.CharMaps.FirstOrDefault(i=>i.Encoding == Encoding.AppleRoman) is {} charMap)
            face.SetCharmap(charMap);
    }
}