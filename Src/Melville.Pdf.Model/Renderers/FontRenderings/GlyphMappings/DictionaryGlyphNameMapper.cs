using System.Collections.Generic;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal interface INameToGlyphMapping
{
    uint GetGlyphFor(PdfDirectObject name);
}

internal partial class DictionaryGlyphNameMapper: INameToGlyphMapping
{
    [FromConstructor] private IReadOnlyDictionary<uint, uint> mappings;

    public uint GetGlyphFor(PdfDirectObject name)
    {
        return mappings.TryGetValue(HashForString(name), out var glyph) ? glyph : 0;
    }

    protected virtual uint HashForString(byte[] name) => FnvHash.FnvHashAsUInt(name);
    protected virtual uint HashForString(PdfDirectObject name) => 
        FnvHash.FnvHashAsUInt(name.Get<StringSpanSource>().GetSpan());
}