using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

public class CustomFontEncoding : IByteToCharacterMapping
{
    private readonly IByteToCharacterMapping baseEncoding;
    private readonly IReadOnlyDictionary<byte, char> customMappings;

    public CustomFontEncoding(IByteToCharacterMapping baseEncoding, IReadOnlyDictionary<byte, char> customMappings)
    {
        this.baseEncoding = baseEncoding;
        this.customMappings = customMappings;
    }

    public uint MapToUnicode(byte input) =>
        customMappings.TryGetValue(input, out var mappedValue) ? 
            mappedValue:
            baseEncoding.MapToUnicode(input);
}