using System;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IGlyphMapping
{
    (uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input);
}

public class UnicodeGlyphMapping : IGlyphMapping
{
    private Face face;
    private IByteToUnicodeMapping charMapping;

    public UnicodeGlyphMapping(Face face, IByteToUnicodeMapping charMapping)
    {
        this.face = face;
        this.charMapping = charMapping;
    }

    public (uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input) =>
        (face.GetCharIndex(charMapping.MapToUnicode(input[0])), 1);
}

 