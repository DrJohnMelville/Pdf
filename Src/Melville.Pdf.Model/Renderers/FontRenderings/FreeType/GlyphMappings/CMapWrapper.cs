using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal class CMapWrapper(ICmapImplementation inner) : IMapCharacterToGlyph
{
    /// <inheritdoc />
    public uint GetGlyph(uint character) => inner.Map(character);
}
