using System;
using System.Collections.Generic;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal interface INameToGlyphMapping
{
#warning get rid of this
    uint GetGlyphFor(byte[] name);
    uint GetGlyphFor(PdfName name);
}

internal partial class DictionaryGlyphNameMapper: INameToGlyphMapping
{
    [FromConstructor] private IReadOnlyDictionary<uint, uint> mappings;
    public uint GetGlyphFor(byte[] name)
    {
        return mappings.TryGetValue(HashForString(name), out var glyph) ? glyph : 0;
    }

    public uint GetGlyphFor(PdfName name)
    {
        return mappings.TryGetValue(HashForString(name), out var glyph) ? glyph : 0;
    }

    protected virtual uint HashForString(byte[] name) => FnvHash.FnvHashAsUInt(name);
    protected virtual uint HashForString(PdfName name) => (uint)name.GetHashCode();
}