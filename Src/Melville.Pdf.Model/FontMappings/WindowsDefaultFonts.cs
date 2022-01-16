using System;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.FontMappings;

public class WindowsDefaultFonts : IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman =
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };

    private static readonly byte[] CourierNew =
        { 67, 111, 117, 114, 105, 101, 114, 78, 101, 119 };

    private static readonly byte[] Arial = { 65, 114, 105, 97, 108 };
    private static readonly byte[] Symbol = { 83, 121, 109, 98, 111, 108 };

    public IFontMapping MapDefaultFont(PdfName font, IByteToUnicodeMapping suggestedEncoding) =>
        font.GetHashCode() switch
        {
            KnownNameKeys.Courier => new NamedDefaultMapping(CourierNew, false, false, suggestedEncoding),
            KnownNameKeys.CourierBold => new NamedDefaultMapping(CourierNew, true, false, suggestedEncoding),
            KnownNameKeys.CourierOblique => new NamedDefaultMapping(CourierNew, false, true, suggestedEncoding),
            KnownNameKeys.CourierBoldOblique => new NamedDefaultMapping(CourierNew, true, true, suggestedEncoding),
            KnownNameKeys.Helvetica => new NamedDefaultMapping(Arial, false, false, suggestedEncoding),
            KnownNameKeys.HelveticaBold => new NamedDefaultMapping(Arial, true, false, suggestedEncoding),
            KnownNameKeys.HelveticaOblique => new NamedDefaultMapping(Arial, false, true, suggestedEncoding),
            KnownNameKeys.HelveticaBoldOblique => new NamedDefaultMapping(Arial, true, true, suggestedEncoding),
            KnownNameKeys.TimesRoman => new NamedDefaultMapping(TimesNewRoman, false, false, suggestedEncoding),
            KnownNameKeys.TimesBold => new NamedDefaultMapping(TimesNewRoman, true, false, suggestedEncoding),
            KnownNameKeys.TimesOblique => new NamedDefaultMapping(TimesNewRoman, false, true, suggestedEncoding),
            KnownNameKeys.TimesBoldOblique => new NamedDefaultMapping(TimesNewRoman, true, true, suggestedEncoding),
            KnownNameKeys.Symbol => SegoeUiSymbolMappedFont.Symbol,
            KnownNameKeys.ZapfDingbats => SegoeUiSymbolMappedFont.ZapfDingbats,
            _ => new NamedDefaultMapping(font.Bytes, false, false, suggestedEncoding)
        };
}