using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

public class CustomFontEncoding : IByteToUnicodeMapping
{
    private readonly IByteToUnicodeMapping baseEncoding;
    private readonly IReadOnlyDictionary<byte, char> customMappings;

    public CustomFontEncoding(IByteToUnicodeMapping baseEncoding, IReadOnlyDictionary<byte, char> customMappings)
    {
        this.baseEncoding = baseEncoding;
        this.customMappings = customMappings;
    }

    public char MapToUnicode(byte input) =>
        customMappings.TryGetValue(input, out var mappedValue) ? 
            mappedValue:
            baseEncoding.MapToUnicode(input);
}

public static class CustomFontEncodingFactory
{
    public static async ValueTask<IByteToUnicodeMapping> Create(
        IByteToUnicodeMapping basis, PdfArray differences)
    {
        var dict = new Dictionary<byte, char>();
        byte currentChar = 0;
        await foreach (var item in differences.ConfigureAwait(false))
        {
            switch (item)
            {
                case PdfNumber num: currentChar = (byte)num.IntValue; break;
                case PdfName name: dict.Add(currentChar++, GlyphNameToUnicodeMap.AdobeGlyphList.Map(name)); break;
            }
        }

        return dict.Count > 0 ? new CustomFontEncoding(basis, dict) : basis;
    }
}