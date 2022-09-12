using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

[StaticSingleton]
public partial class WindowsDefaultFonts : IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman =
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };

    private static readonly byte[] CourierNew =
        { 67, 111, 117, 114, 105, 101, 114, 78, 101, 119 };

    private static readonly byte[] Arial = { 65, 114, 105, 97, 108 };
    private static readonly byte[] SegoeUISymbol =
        { 83, 101, 103, 111, 101, 32, 85, 73, 32, 83, 121, 109, 98, 111, 108 };

    public async ValueTask<IRealizedFont>  FontFromName(
        PdfName font, FontFlags fontFlags, FreeTypeFontFactory factory)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => await SystemFont(CourierNew, factory,  false, false).CA(),
            KnownNameKeys.CourierBold => await SystemFont(CourierNew, factory,  true, false).CA(),
            KnownNameKeys.CourierOblique => await SystemFont(CourierNew, factory,  false, true).CA(),
            KnownNameKeys.CourierBoldOblique => await SystemFont(CourierNew, factory,  true, true).CA(),
            KnownNameKeys.Helvetica => await SystemFont(Arial, factory,  false, false).CA(),
            KnownNameKeys.HelveticaBold => await SystemFont(Arial, factory,  true, false).CA(),
            KnownNameKeys.HelveticaOblique => await SystemFont(Arial, factory,  false, true).CA(),
            KnownNameKeys.HelveticaBoldOblique => await SystemFont(Arial, factory,  true, true).CA(),
            KnownNameKeys.TimesRoman => await SystemFont(TimesNewRoman, factory,  false, false).CA(),
            KnownNameKeys.TimesBold => await SystemFont(TimesNewRoman, factory,  true, false).CA(),
            KnownNameKeys.TimesOblique => await SystemFont(TimesNewRoman, factory,  false, true).CA(),
            KnownNameKeys.TimesBoldOblique => await SystemFont(TimesNewRoman, factory,  true, true).CA(),
            KnownNameKeys.Symbol => await SystemFont(SegoeUISymbol, factory,  false, false).CA(), 
            KnownNameKeys.ZapfDingbats => await SystemFont(SegoeUISymbol, factory,  false, false).CA(),
            _ => await TrySystemFont(font.Bytes, factory, 
                        fontFlags.HasFlag(FontFlags.ForceBold), fontFlags.HasFlag(FontFlags.Italic)).CA()??
                 await FontFromName(fontFlags.MapBuiltInFont(), fontFlags, factory).CA()
        };
    }

    private  ValueTask<IRealizedFont> SystemFont(
        byte[] name, FreeTypeFontFactory factory, bool bold, bool italic)
    {
        var fontReference = GlobalFreeTypeResources.SystemFontLibrary().FontFromName(name, bold, italic);
        return fontReference?.ReaiizeUsing(factory) 
               ?? throw new IOException("Could not find required font file.");
    }
    private  async ValueTask<IRealizedFont?> TrySystemFont(
        byte[] name, FreeTypeFontFactory factory, bool bold, bool italic)
    {
        var fontReference = GlobalFreeTypeResources.SystemFontLibrary().FontFromName(name, bold, italic);
        if (fontReference is null) return null;
        return await fontReference.ReaiizeUsing(factory).CA();
    }
}