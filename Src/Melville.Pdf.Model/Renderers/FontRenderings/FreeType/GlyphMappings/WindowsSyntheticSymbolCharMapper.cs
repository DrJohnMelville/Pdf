using System.Collections.Generic;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal class WindowsSyntheticSymbolCharMapper(ICmapImplementation inner): ICmapImplementation
{
    public static ICmapImplementation? Wrap(ICmapImplementation? inner) =>
        inner is null? null: new WindowsSyntheticSymbolCharMapper(inner);

    public IEnumerable<(int Bytes, uint Character, uint Glyph)> AllMappings() => inner.AllMappings();

    public bool TryMap(int bytes, uint character, out uint glyph) => (inner.TryMap(bytes, character, out glyph)) ||
            inner.TryMap(bytes, character | 0xF000, out glyph);
}
