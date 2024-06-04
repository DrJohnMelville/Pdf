namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal class CFFGlyphSource : IGlyphSource
{
    private readonly CffIndex glyphs;
    internal CFFGlyphSource(CffIndex glyphs) => this.glyphs = glyphs;

    public int GlyphCount => (int)glyphs.Length;
}