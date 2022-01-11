using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.FakeUris;

namespace Melville.Pdf.Wpf;

public readonly struct RealizedFontFactory
{
    private readonly IFontMapping mapping;
    private readonly TempFontDirectory fontCache;
    private readonly IFontWriteTarget<GeometryGroup> target;

    public RealizedFontFactory(
        IFontMapping mapping, TempFontDirectory fontCache, IFontWriteTarget<GeometryGroup> target)
    {
        this.mapping = mapping;
        this.fontCache = fontCache;
        this.target = target;
    }

    public async ValueTask<IRealizedFont> CreateRealizedFont(double size)
    {
        return new WpfRealizedTrueOrOpetypeFont(
            GetGlyphTypeface(await CreateWpfTypeface()), size, mapping.Mapping, target);
    }
    private GlyphTypeface GetGlyphTypeface(Typeface typeface) =>
        typeface.TryGetGlyphTypeface(out var gtf) ? gtf
            : throw new PdfParseException("Cannot create true/open type font.");

    private  async ValueTask<Typeface> CreateWpfTypeface() =>
        new Typeface(await FindFontFamily(mapping.Font),
            mapping.Oblique ? FontStyles.Italic : FontStyles.Normal,
            mapping.Bold ? FontWeights.Bold : FontWeights.Normal,
            FontStretches.Normal);

    private  async ValueTask<FontFamily> FindFontFamily(object font) =>
        font switch
        {
            byte[] fontName => FindFontFamily(fontName),
            PdfStream s => Fonts.GetFontFamilies(await fontCache.StoreStream(s)).First(),
            _ => throw new PdfParseException("Cannot create a font from: " + font)
        };

    private static FontFamily FindFontFamily(byte[] fontName)
    {
        int currentLen = -1;
        FontFamily? result = null;
        foreach (var family in Fonts.SystemFontFamilies)
        {
            var len = fontName.CommonPrefixLength(family.Source);
            if (len > currentLen || 
                (len == currentLen && family.Source.Length < (result?.Source.Length??1000)))
            {
                currentLen = len;
                result = family;
            }
        }
        return result ?? new FontFamily("Arial");
    }
}