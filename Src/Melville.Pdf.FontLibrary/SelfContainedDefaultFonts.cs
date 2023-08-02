using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.FontLibrary;

/// <summary>
/// Map default fonts to a set of fonts contained as resources in this assembly
/// </summary>
[StaticSingleton]
public partial class SelfContainedDefaultFonts: IDefaultFontMapper
{
    /// <inheritdoc />
    public DefaultFontReference FontFromName(PdfDirectObject font, FontFlags fontFlags)
    {
        return font switch
        {
            var x when x.Equals(KnownNames.CourierTName) => SystemFont("Courier Prime.ttf"),
            var x when x.Equals(KnownNames.CourierBoldTName) => SystemFont("Courier Prime Bold.ttf"),
            var x when x.Equals(KnownNames.CourierObliqueTName) => SystemFont("Courier Prime Italic.ttf"),
            var x when x.Equals(KnownNames.CourierBoldObliqueTName) => SystemFont("Courier Prime Bold Italic.ttf"),
            var x when x.Equals(KnownNames.HelveticaTName) => SystemFont("Roboto-Regular.ttf"),
            var x when x.Equals(KnownNames.HelveticaBoldTName) => SystemFont("Roboto-Bold.ttf"),
            var x when x.Equals(KnownNames.HelveticaObliqueTName) => SystemFont("Roboto-Italic.ttf"),
            var x when x.Equals(KnownNames.HelveticaBoldObliqueTName) => SystemFont("Roboto-BoldItalic.ttf"),
            var x when x.Equals(KnownNames.TimesRomanTName) => SystemFont("LinLibertine_R.otf"),
            var x when x.Equals(KnownNames.TimesBoldTName) => SystemFont("LinLibertine_RB.otf"),
            var x when x.Equals(KnownNames.TimesObliqueTName) => SystemFont("LinLibertine_RI.otf"),
            var x when x.Equals(KnownNames.TimesBoldObliqueTName) => SystemFont("LinLibertine_RBI.otf"),
            var x when x.Equals(KnownNames.SymbolTName) => SystemFont("Symbola.ttf"), 
            var x when x.Equals(KnownNames.ZapfDingbatsTName) => SystemFont("Symbola.ttf"),
            _ => FontFromName(fontFlags.MapBuiltInFont(), fontFlags)
        };
    }

    private DefaultFontReference SystemFont(string fileName) =>
        new DefaultFontReference(
            GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary." + fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName), 0);
}