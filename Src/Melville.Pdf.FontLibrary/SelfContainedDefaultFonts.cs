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
    public ValueTask<DefaultFontReference> FontFromNameAsync(PdfDirectObject font, FontFlags fontFlags)
    {
        return font switch
        {
            var x when x.Equals(KnownNames.Courier) => SystemFontAsync("Courier Prime.ttf"),
            var x when x.Equals(KnownNames.CourierBold) => SystemFontAsync("Courier Prime Bold.ttf"),
            var x when x.Equals(KnownNames.CourierOblique) => SystemFontAsync("Courier Prime Italic.ttf"),
            var x when x.Equals(KnownNames.CourierBoldOblique) => SystemFontAsync("Courier Prime Bold Italic.ttf"),
            var x when x.Equals(KnownNames.CourierItalic) => SystemFontAsync("Courier Prime Italic.ttf"),
            var x when x.Equals(KnownNames.CourierBoldItalic) => SystemFontAsync("Courier Prime Bold Italic.ttf"),
            var x when x.Equals(KnownNames.Helvetica) => SystemFontAsync("Roboto-Regular.ttf"),
            var x when x.Equals(KnownNames.HelveticaBold) => SystemFontAsync("Roboto-Bold.ttf"),
            var x when x.Equals(KnownNames.HelveticaOblique) => SystemFontAsync("Roboto-Italic.ttf"),
            var x when x.Equals(KnownNames.HelveticaBoldOblique) => SystemFontAsync("Roboto-BoldItalic.ttf"),
            var x when x.Equals(KnownNames.HelveticaItalic) => SystemFontAsync("Roboto-Italic.ttf"),
            var x when x.Equals(KnownNames.HelveticaBoldItalic) => SystemFontAsync("Roboto-BoldItalic.ttf"),
            var x when x.Equals(KnownNames.TimesRoman) => SystemFontAsync("LinLibertine_R.otf"),
            var x when x.Equals(KnownNames.TimesBold) => SystemFontAsync("LinLibertine_RB.otf"),
            var x when x.Equals(KnownNames.TimesOblique) => SystemFontAsync("LinLibertine_RI.otf"),
            var x when x.Equals(KnownNames.TimesBoldOblique) => SystemFontAsync("LinLibertine_RBI.otf"),
            var x when x.Equals(KnownNames.TimesItalic) => SystemFontAsync("LinLibertine_RI.otf"),
            var x when x.Equals(KnownNames.TimesBoldItalic) => SystemFontAsync("LinLibertine_RBI.otf"),
            var x when x.Equals(KnownNames.Symbol) => SystemFontAsync("Symbola.ttf"), 
            var x when x.Equals(KnownNames.ZapfDingbats) => SystemFontAsync("Symbola.ttf"),
            _ => FontFromNameAsync(fontFlags.MapBuiltInFont(), fontFlags)
        };
    }

    private ValueTask<DefaultFontReference> SystemFontAsync(string fileName) =>
        new( new DefaultFontReference(
            FontStream(fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName), 0));

    private Stream? FontStream(string fileName) => 
        GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary." + fileName);
}