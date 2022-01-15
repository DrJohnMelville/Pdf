using System;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
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
        KnownNameKeys.Courier => new NamedDefaultMapping(CourierNew, false, false, CharacterEncodings.Standard),
        KnownNameKeys.CourierBold => new NamedDefaultMapping(CourierNew, true, false, CharacterEncodings.Standard),
        KnownNameKeys.CourierOblique => new NamedDefaultMapping(CourierNew, false, true, CharacterEncodings.Standard),
        KnownNameKeys.CourierBoldOblique => new NamedDefaultMapping(CourierNew, true, true, CharacterEncodings.Standard),
        KnownNameKeys.Helvetica => new NamedDefaultMapping(Arial, false, false, CharacterEncodings.Standard),
        KnownNameKeys.HelveticaBold => new NamedDefaultMapping(Arial, true, false, CharacterEncodings.Standard),
        KnownNameKeys.HelveticaOblique => new NamedDefaultMapping(Arial, false, true, CharacterEncodings.Standard),
        KnownNameKeys.HelveticaBoldOblique => new NamedDefaultMapping(Arial, true, true, CharacterEncodings.Standard),
        KnownNameKeys.TimesRoman => new NamedDefaultMapping(TimesNewRoman, false, false, CharacterEncodings.Standard),
        KnownNameKeys.TimesBold => new NamedDefaultMapping(TimesNewRoman, true, false, CharacterEncodings.Standard),
        KnownNameKeys.TimesOblique => new NamedDefaultMapping(TimesNewRoman, false, true, CharacterEncodings.Standard),
        KnownNameKeys.TimesBoldOblique => new NamedDefaultMapping(TimesNewRoman, true, true, CharacterEncodings.Standard),
        KnownNameKeys.Symbol => new NamedDefaultMapping(Symbol, false, false, SymbolMapping.Instance),
        KnownNameKeys.ZapfDingbats => new DingbatsToSegoeUi(),
        _ => new NamedDefaultMapping(font.Bytes, false, false, CharacterEncodings.Standard)
    };
}

public class SymbolMapping: IByteToUnicodeMapping
{
    public  static readonly SymbolMapping Instance = new();

    private SymbolMapping()
    {
    }

    public char MapToUnicode(byte input)
    {
        return input == 0xA0 ? (char)0x20ac : (char)input;
    }
}

public class NullUnicodeMapping : IByteToUnicodeMapping
{
    public static readonly NullUnicodeMapping Instance = new();
    private NullUnicodeMapping()
    {
    }

    public char MapToUnicode(byte input) => (char)input;
}


public class NamedDefaultMapping : IFontMapping
{
    private object fontData;
    public bool Bold { get; }
    public bool Oblique { get; }
    

    public NamedDefaultMapping(object fontData, bool bold, bool oblique, IByteToUnicodeMapping mapping)
    {
        this.fontData = fontData;
        Bold = bold;
        Oblique = oblique;
        Mapping = mapping;
    }

    public object Font => fontData;
    public IByteToUnicodeMapping Mapping { get; }
}