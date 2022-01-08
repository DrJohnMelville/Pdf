using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.FontMappings;


public class WindowsDefaultFonts: IDefaultFontMapper
{
    private static readonly byte[] TimesNewRoman = 
        { 84, 105, 109, 101, 115, 78, 101, 119, 82, 111, 109, 97, 110 };
    private static readonly byte[] CourierNew =
        { 67, 111, 117, 114, 105, 101, 114, 78, 101, 119 };
    private static readonly byte[] Arial = { 65, 114, 105, 97, 108 };
    private static readonly byte[] Symbol = { 83, 121, 109, 98, 111, 108 };
    public IFontMapping MapDefaultFont(PdfName font) => font.GetHashCode() switch
    {
        KnownNameKeys.Courier => new NamedDefaultMapping(CourierNew, false, false),
        KnownNameKeys.CourierBold => new NamedDefaultMapping(CourierNew, true, false),
        KnownNameKeys.CourierOblique => new NamedDefaultMapping(CourierNew, false, true),
        KnownNameKeys.CourierBoldOblique => new NamedDefaultMapping(CourierNew, true, true),
        KnownNameKeys.Helvetica => new NamedDefaultMapping(Arial, false, false),
        KnownNameKeys.HelveticaBold => new NamedDefaultMapping(Arial, true, false),
        KnownNameKeys.HelveticaOblique => new NamedDefaultMapping(Arial, false, true),
        KnownNameKeys.HelveticaBoldOblique => new NamedDefaultMapping(Arial, true, true),
        KnownNameKeys.TimesRoman => new NamedDefaultMapping(TimesNewRoman, false, false),
        KnownNameKeys.TimesBold => new NamedDefaultMapping(TimesNewRoman, true, false),
        KnownNameKeys.TimesOblique => new NamedDefaultMapping(TimesNewRoman, false, true),
        KnownNameKeys.TimesBoldOblique => new NamedDefaultMapping(TimesNewRoman, true, true),
        KnownNameKeys.Symbol => new NamedDefaultMapping(Symbol, false, false),
        KnownNameKeys.ZapfDingbats => new DingbatsToSegoeUi(),
        _ => new NamedDefaultMapping(font.Bytes, false, false)
    };
}

public class NamedDefaultMapping : IFontMapping, IByteToUnicodeMapping
{
    private object fontData;
    public bool Bold { get; }
    public bool Oblique { get; }

    public NamedDefaultMapping(object fontData, bool bold, bool oblique)
    {
        this.fontData = fontData;
        Bold = bold;
        Oblique = oblique;
    }

    public object Font => fontData;
    public IByteToUnicodeMapping Mapping => this;
    public char MapToUnicode(byte input) => (char)input;
}