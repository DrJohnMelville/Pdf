using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.FontLibrary;

/// <summary>
/// Map default fonts to a set of fonts contained as resources in this assembly
/// </summary>
[StaticSingleton]
public partial class SelfContainedDefaultFonts: IDefaultFontMapper
{
    /// <inheritdoc />
    public DefaultFontReference  FontFromName(
        PdfName font, FontFlags fontFlags)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => SystemFont("Courier Prime.ttf"),
            KnownNameKeys.CourierBold => SystemFont("Courier Prime Bold.ttf"),
            KnownNameKeys.CourierOblique => SystemFont("Courier Prime Italic.ttf"),
            KnownNameKeys.CourierBoldOblique => SystemFont("Courier Prime Bold Italic.ttf"),
            KnownNameKeys.Helvetica => SystemFont("Roboto-Regular.ttf"),
            KnownNameKeys.HelveticaBold => SystemFont("Roboto-Bold.ttf"),
            KnownNameKeys.HelveticaOblique => SystemFont("Roboto-Italic.ttf"),
            KnownNameKeys.HelveticaBoldOblique => SystemFont("Roboto-BoldItalic.ttf"),
            KnownNameKeys.TimesRoman => SystemFont("LinLibertine_R.otf"),
            KnownNameKeys.TimesBold => SystemFont("LinLibertine_RB.otf"),
            KnownNameKeys.TimesOblique => SystemFont("LinLibertine_RI.otf"),
            KnownNameKeys.TimesBoldOblique => SystemFont("LinLibertine_RBI.otf"),
            KnownNameKeys.Symbol => SystemFont("Symbola.ttf"), 
            KnownNameKeys.ZapfDingbats => SystemFont("Symbola.ttf"),
            _ => FontFromName(fontFlags.MapBuiltInFont(), fontFlags)
        };
    }

    private DefaultFontReference SystemFont(string fileName) =>
        new DefaultFontReference(
            GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary." + fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName), 0);
}