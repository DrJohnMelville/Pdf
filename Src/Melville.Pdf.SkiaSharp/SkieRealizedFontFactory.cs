using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

namespace Melville.Pdf.SkiaSharp;

public readonly struct SkieRealizedFontFactory
{
    private readonly IFontMapping font;
    private readonly IFontWriteTarget<SKPath> target;

    public SkieRealizedFontFactory(IFontMapping font, IFontWriteTarget<SKPath> target)
    {
        this.font = font;
        this.target = target;
    }

    public async ValueTask<IRealizedFont> CreateRealizedFontAsync(double size) => new SkiaRealizedFont(
        (await CreateSkTypeface()).ToFont((float)size), target, font.Mapping);

    private async ValueTask<SKTypeface> CreateSkTypeface() => font.Font switch
    {
        byte[] fontName => SKTypeface.FromFamilyName(FindFontFamily(fontName),
            font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            font.Oblique ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright),
        PdfStream fontFile => SKTypeface.FromStream(await fontFile.StreamContentAsync()),
        _ => throw new PdfParseException("Cannot create font from: " + font.Font)
    };
    
    private static string FindFontFamily(byte[] fontName)
    {
        var result = "Arial";
        var currentLen = -1;
        foreach (var family in SKFontManager.Default.FontFamilies)
        {
            var len = fontName.CommonPrefixLength(family);
            if (len > currentLen || 
                len == currentLen && family.Length < result.Length)
            {
                currentLen = len;
                result = family;
            }
        }
        return result;
    }
    

}