using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IGlyphMapping
{
    uint SelectGlyph(byte stringByte);
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

    public uint SelectGlyph(byte stringByte) => 
        face.GetCharIndex(charMapping.MapToUnicode(stringByte));
}