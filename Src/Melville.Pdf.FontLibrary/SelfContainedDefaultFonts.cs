using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
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
    public DefaultFontReference FontFromName(PdfDirectValue font, FontFlags fontFlags)
    {
        return font switch
        {
            var x when x.Equals(KnownNames.Courier) => SystemFont("Courier Prime.ttf"),
            var x when x.Equals(KnownNames.CourierBold) => SystemFont("Courier Prime Bold.ttf"),
            var x when x.Equals(KnownNames.CourierOblique) => SystemFont("Courier Prime Italic.ttf"),
            var x when x.Equals(KnownNames.CourierBoldOblique) => SystemFont("Courier Prime Bold Italic.ttf"),
            var x when x.Equals(KnownNames.Helvetica) => SystemFont("Roboto-Regular.ttf"),
            var x when x.Equals(KnownNames.HelveticaBold) => SystemFont("Roboto-Bold.ttf"),
            var x when x.Equals(KnownNames.HelveticaOblique) => SystemFont("Roboto-Italic.ttf"),
            var x when x.Equals(KnownNames.HelveticaBoldOblique) => SystemFont("Roboto-BoldItalic.ttf"),
            var x when x.Equals(KnownNames.TimesRoman) => SystemFont("LinLibertine_R.otf"),
            var x when x.Equals(KnownNames.TimesBold) => SystemFont("LinLibertine_RB.otf"),
            var x when x.Equals(KnownNames.TimesOblique) => SystemFont("LinLibertine_RI.otf"),
            var x when x.Equals(KnownNames.TimesBoldOblique) => SystemFont("LinLibertine_RBI.otf"),
            var x when x.Equals(KnownNames.Symbol) => SystemFont("Symbola.ttf"), 
            var x when x.Equals(KnownNames.ZapfDingbats) => SystemFont("Symbola.ttf"),
            _ => FontFromName(fontFlags.MapBuiltInFont(), fontFlags)
        };
    }

    private DefaultFontReference SystemFont(string fileName) =>
        new DefaultFontReference(
            GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary." + fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName), 0);
}