using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;

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

[Obsolete]
public static class CustomFontEncodingFactory
{
    public static async ValueTask<IByteToCharacterMapping> Create(
        IByteToCharacterMapping basis, PdfArray differences, IGlyphNameMap glypnNames)
    {
        var dict = await DifferenceArrayToMapAsync(differences, glypnNames).CA();
        return dict.Count > 0 ? new CustomFontEncoding(basis, dict) : basis;
    }

    public static async ValueTask<Dictionary<byte, char>> DifferenceArrayToMapAsync(
        PdfArray differences, IGlyphNameMap nameMap)
    {
        var dict = new Dictionary<byte, char>();
        byte currentChar = 0;
        await foreach (var item in differences.CA())
        {
            switch (item)
            {
                case PdfNumber num:
                    currentChar = (byte)num.IntValue;
                    break;
                case PdfName name:
                    if (nameMap.TryMap(name, out var character)) dict.Add(currentChar, character);
                    currentChar++;
                    break;
            }
        }
        return dict;
    }
}