using System;

namespace Melville.Pdf.Model.FontMappings;


public class WindowsDefaultFonts: IDefaultFontMapper
{
    public IFontMapping MapDefaultFont(DefaultPdfFonts font) =>
        font switch {
            DefaultPdfFonts.Courier => new NamedDefaultMapping("Courier New"),
            DefaultPdfFonts.Helvetica => new NamedDefaultMapping("Arial"),
            DefaultPdfFonts.Times => new NamedDefaultMapping("Times New Roman"),
            DefaultPdfFonts.Symbol => new NamedDefaultMapping("Symbol"),
            DefaultPdfFonts.Dingbats => new DingbatsToSegoeUi(),
            _ => throw new ArgumentOutOfRangeException(nameof(font), font, null)
        };
}

public class NamedDefaultMapping : IFontMapping, IByteToUnicodeMapping
{
    private string osFontName;

    public NamedDefaultMapping(string osFontName)
    {
        this.osFontName = osFontName;
    }

    public object Font => osFontName;
    public IByteToUnicodeMapping Mapping => this;
    public char MapToUnicode(byte input) => (char)input;
}