using System;
using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public partial class DictionaryGlyphMapping : IGlyphMapping
{
    [FromConstructor] private IReadOnlyDictionary<byte, char> mappings;

    public (uint character, uint glyph, int bytesConsumed) SelectGlyph(in ReadOnlySpan<byte> input)
    {
        var character = input[0];
        if (!mappings.TryGetValue(character, out var glyph)) glyph = (char)character;
        return (character, glyph, 1);
    }
}