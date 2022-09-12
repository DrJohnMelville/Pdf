using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.FontLibrary;

[StaticSingleton]
public partial class SelfContainedDefaultFonts: IDefaultFontMapper
{
    public ValueTask<IRealizedFont>  FontFromName(
        PdfName font, FontFlags fontFlags, FreeTypeFontFactory factory)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => SystemFont("Courier Prime.ttf", factory),
            KnownNameKeys.CourierBold => SystemFont("Courier Prime Bold.ttf", factory),
            KnownNameKeys.CourierOblique => SystemFont("Courier Prime Italic.ttf", factory),
            KnownNameKeys.CourierBoldOblique => SystemFont("Courier Prime Bold Italic.ttf", factory),
            KnownNameKeys.Helvetica => SystemFont("Roboto-Regular.ttf", factory),
            KnownNameKeys.HelveticaBold => SystemFont("Roboto-Bold.ttf", factory),
            KnownNameKeys.HelveticaOblique => SystemFont("Roboto-Italic.ttf", factory),
            KnownNameKeys.HelveticaBoldOblique => SystemFont("Roboto-BoldItalic.ttf", factory),
            KnownNameKeys.TimesRoman => SystemFont("LinLibertine_R.otf", factory),
            KnownNameKeys.TimesBold => SystemFont("LinLibertine_RB.otf", factory),
            KnownNameKeys.TimesOblique => SystemFont("LinLibertine_RI.otf", factory),
            KnownNameKeys.TimesBoldOblique => SystemFont("LinLibertine_RBI.otf", factory),
            KnownNameKeys.Symbol => SystemFont("Symbola.ttf", factory), 
            KnownNameKeys.ZapfDingbats => SystemFont("Symbola.ttf", factory),
            _ => FontFromName(fontFlags.MapBuiltInFont(), fontFlags, factory)
        };
    }

    private ValueTask<IRealizedFont> SystemFont(string fileName, FreeTypeFontFactory factory) =>
        factory.FromCSharpStream(
            GetType().Assembly.GetManifestResourceStream("Melville.Pdf.FontLibrary." + fileName) ??
            throw new InvalidOperationException("Cannot find font resource: " + fileName));
}