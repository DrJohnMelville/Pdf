using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal class CompositeGlyphNameMapper : INameToGlyphMapping
{
    private INameToGlyphMapping?[] mappings;

    public CompositeGlyphNameMapper(params INameToGlyphMapping?[] mappings)
    {
        this.mappings = mappings;
    }

    public uint GetGlyphFor(byte[] name)
    {
        foreach (var mapping in mappings)
        {
            if (mapping is null) continue;
            var glyph = mapping.GetGlyphFor(name);
            if (glyph > 0) return glyph;
        }
        return 0;
    }
}