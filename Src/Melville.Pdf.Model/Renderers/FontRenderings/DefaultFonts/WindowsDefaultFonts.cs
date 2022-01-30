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

    public ValueTask<IRealizedFont>  MapDefaultFont(PdfName font, double size, IByteToUnicodeMapping suggestedEncoding) =>
        font.GetHashCode() switch
        {
            KnownNameKeys.Courier => FreeTypeFontFactory.SystemFont(CourierNew, size, suggestedEncoding, false, false),
            KnownNameKeys.CourierBold => FreeTypeFontFactory.SystemFont(CourierNew, size, suggestedEncoding, true, false),
            KnownNameKeys.CourierOblique => FreeTypeFontFactory.SystemFont(CourierNew, size, suggestedEncoding, false, true),
            KnownNameKeys.CourierBoldOblique =>FreeTypeFontFactory.SystemFont(CourierNew, size, suggestedEncoding, true, true),
            KnownNameKeys.Helvetica => FreeTypeFontFactory.SystemFont(Arial, size, suggestedEncoding, false, false),
            KnownNameKeys.HelveticaBold => FreeTypeFontFactory.SystemFont(Arial, size, suggestedEncoding, true, false),
            KnownNameKeys.HelveticaOblique => FreeTypeFontFactory.SystemFont(Arial, size, suggestedEncoding, false, true),
            KnownNameKeys.HelveticaBoldOblique => FreeTypeFontFactory.SystemFont(Arial, size, suggestedEncoding, true, true),
            KnownNameKeys.TimesRoman => FreeTypeFontFactory.SystemFont(TimesNewRoman, size, suggestedEncoding, false, false),
            KnownNameKeys.TimesBold => FreeTypeFontFactory.SystemFont(TimesNewRoman, size, suggestedEncoding, true, false),
            KnownNameKeys.TimesOblique => FreeTypeFontFactory.SystemFont(TimesNewRoman, size, suggestedEncoding, false, true),
            KnownNameKeys.TimesBoldOblique => FreeTypeFontFactory.SystemFont(TimesNewRoman, size, suggestedEncoding, true, true),
            KnownNameKeys.Symbol => FreeTypeFontFactory.SystemFont(SegoeUISymbol, size, CharacterEncodings.Symbol, false, false),
            KnownNameKeys.ZapfDingbats => FreeTypeFontFactory.SystemFont(SegoeUISymbol, size, ZapfDignbatsMapping.Instance, false, false),
            _ => FreeTypeFontFactory.SystemFont(font.Bytes, size, suggestedEncoding, false, false)
        };
}