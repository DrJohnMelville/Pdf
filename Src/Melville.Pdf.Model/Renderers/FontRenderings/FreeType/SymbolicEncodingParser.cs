using System;
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
        face.SelectCharmap(Encoding.AppleRoman);
        return new (new UnicodeGlyphMapping(face, new PassthroughMapping()));
    }
}