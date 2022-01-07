using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.FontMappings;


public class WindowsDefaultFonts: IDefaultFontMapper
{
    public IFontMapping MapDefaultFont(PdfName font) => font.GetHashCode() switch
    {
        KnownNameKeys.Courier => new NamedDefaultMapping("Courier New", false, false),
        KnownNameKeys.CourierBold => new NamedDefaultMapping("Courier New", true, false),
        KnownNameKeys.CourierOblique => new NamedDefaultMapping("Courier New", false, true),
        KnownNameKeys.CourierBoldOblique => new NamedDefaultMapping("Courier New", true, true),
        KnownNameKeys.Helvetica => new NamedDefaultMapping("Arial", false, false),
        KnownNameKeys.HelveticaBold => new NamedDefaultMapping("Arial", true, false),
        KnownNameKeys.HelveticaOblique => new NamedDefaultMapping("Arial", false, true),
        KnownNameKeys.HelveticaBoldOblique => new NamedDefaultMapping("Arial", true, true),
        KnownNameKeys.TimesRoman => new NamedDefaultMapping("Times New Roman", false, false),
        KnownNameKeys.TimesBold => new NamedDefaultMapping("Times New Roman", true, false),
        KnownNameKeys.TimesOblique => new NamedDefaultMapping("Times New Roman", false, true),
        KnownNameKeys.TimesBoldOblique => new NamedDefaultMapping("Times New Roman", true, true),
        KnownNameKeys.Symbol => new NamedDefaultMapping("Symbol", false, false),
        KnownNameKeys.ZapfDingbats => new DingbatsToSegoeUi(),
        _ => new NamedDefaultMapping(font.ToString(), false, false)
    };
}

public class NamedDefaultMapping : IFontMapping, IByteToUnicodeMapping
{
    private string osFontName;
    public bool Bold { get; }
    public bool Oblique { get; }

    public NamedDefaultMapping(string osFontName, bool bold, bool oblique)
    {
        this.osFontName = osFontName;
        Bold = bold;
        Oblique = oblique;
    }

    public object Font => osFontName;
    public IByteToUnicodeMapping Mapping => this;
    public char MapToUnicode(byte input) => (char)input;
}