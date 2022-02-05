using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

public class WindowsDefaultFonts : IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman =
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };

    private static readonly byte[] CourierNew =
        { 67, 111, 117, 114, 105, 101, 114, 78, 101, 119 };

    private static readonly byte[] Arial = { 65, 114, 105, 97, 108 };
    private static readonly byte[] SegoeUISymbol =
        { 83, 101, 103, 111, 101, 32, 85, 73, 32, 83, 121, 109, 98, 111, 108 };

    public ValueTask<IRealizedFont>  MapDefaultFont(PdfName font, FreeTypeFontFactory factory)
    {
        return font.GetHashCode() switch
        {
            KnownNameKeys.Courier => factory.SystemFont(CourierNew, false, false),
            KnownNameKeys.CourierBold => factory.SystemFont(CourierNew, true, false),
            KnownNameKeys.CourierOblique => factory.SystemFont(CourierNew, false, true),
            KnownNameKeys.CourierBoldOblique => factory.SystemFont(CourierNew, true, true),
            KnownNameKeys.Helvetica => factory.SystemFont(Arial, false, false),
            KnownNameKeys.HelveticaBold => factory.SystemFont(Arial, true, false),
            KnownNameKeys.HelveticaOblique => factory.SystemFont(Arial, false, true),
            KnownNameKeys.HelveticaBoldOblique => factory.SystemFont(Arial, true, true),
            KnownNameKeys.TimesRoman => factory.SystemFont(TimesNewRoman, false, false),
            KnownNameKeys.TimesBold => factory.SystemFont(TimesNewRoman, true, false),
            KnownNameKeys.TimesOblique => factory.SystemFont(TimesNewRoman, false, true),
            KnownNameKeys.TimesBoldOblique => factory.SystemFont(TimesNewRoman, true, true),
            KnownNameKeys.Symbol => (factory with{Mapping =  CharacterEncodings.Symbol}).SystemFont(SegoeUISymbol,
                false, false),
            KnownNameKeys.ZapfDingbats => (factory with{Mapping =  ZapfDignbatsMapping.Instance})
                .SystemFont(SegoeUISymbol, false, false),
            _ => factory.SystemFont(font.Bytes, false, false)
        };
    }
}