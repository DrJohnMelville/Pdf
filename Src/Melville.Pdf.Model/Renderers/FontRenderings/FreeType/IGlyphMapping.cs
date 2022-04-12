using System;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using SharpFont;
using SharpFont.PostScript;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IGlyphMapping
{
    (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input);
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

    public (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        var character = input[0];
        return (character, face.GetCharIndex(charMapping.MapToUnicode(character)), 1);
    }
}

public class IdentityCmapMapping: IGlyphMapping
{
    public static readonly IGlyphMapping Instance = new IdentityCmapMapping();
    private IdentityCmapMapping() { }
    public (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        return input.Length > 1 ? DuplicateVal((uint)(input[0] << 8) | input[1], 2) : DuplicateVal(input[0], 1);
    }

    private (uint character, uint glyph, int bytesConsumed) DuplicateVal(uint character, int length) =>
        (character, character, length);
}