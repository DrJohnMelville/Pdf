using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal interface INameToGlyphMapping
{
    uint GetGlyphFor(byte[] name);
}

internal partial class DictionaryGlyphNameMapper: INameToGlyphMapping
{
    [FromConstructor] private IReadOnlyDictionary<uint, uint> mappings;
    public uint GetGlyphFor(byte[] name)
    {
        return mappings.TryGetValue(HashForString(name), out var glyph) ? glyph : 0;
    }

    protected virtual uint HashForString(byte[] name) => FnvHash.FnvHashAsUint(name);
}