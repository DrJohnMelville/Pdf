using System.Collections.Generic;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal class CompositeMapper(IReadOnlyList<IMapCharacterToGlyph> mappers) : IMapCharacterToGlyph
{
    /// <inheritdoc />
    public uint GetGlyph(uint character)
    {
        foreach (var mapper in mappers)
        {
            if (mapper.GetGlyph(character) is var val and > 0) return val;
        }

        return 0;
    }
}
