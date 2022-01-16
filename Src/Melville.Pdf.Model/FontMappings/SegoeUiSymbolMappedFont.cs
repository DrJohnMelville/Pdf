using Melville.Pdf.LowLevel.Model.CharacterEncoding;

namespace Melville.Pdf.Model.FontMappings;

public class SegoeUiSymbolMappedFont : IFontMapping
{
    public static readonly SegoeUiSymbolMappedFont ZapfDingbats = new(ZapfDignbatsMapping.Instance);
    public static readonly SegoeUiSymbolMappedFont Symbol = new SegoeUiSymbolMappedFont(CharacterEncodings.Symbol);
    private SegoeUiSymbolMappedFont(IByteToUnicodeMapping mapping)
    {
        Mapping = mapping;
    }

    private static readonly byte[] SegoeUISymbol =
        { 83, 101, 103, 111, 101, 32, 85, 73, 32, 83, 121, 109, 98, 111, 108 };
    public object Font => SegoeUISymbol;
    public IByteToUnicodeMapping Mapping { get; }

    public bool Bold => false;
    public bool Oblique => false;

}

public class ZapfDignbatsMapping : IByteToUnicodeMapping
{
    public static readonly ZapfDignbatsMapping Instance = new();
    private ZapfDignbatsMapping() { }
    public char MapToUnicode(byte input) =>(char)(input switch
    {
        <0x20 => 0x25A1,
        0x20=> 0X20,
        0x25=> 0x260E,
        0x2a=> 0x261B,
        0x2B=> 0x261E,
        0x48=> 0x26E5, 
        0x6c => 0x25CF,
        0x6e => 0x2587,
        0x74 => 0x25BC,
        0x73 => 0x25B2,
        0x75 => 0x2666,
        0x77 => 0x25d7,
        > 0x7e and < 0xa1 => 0x25A1, 
        0xa8 => 0x2663, // clubs
        0xa9 =>  0x2666,// Diamonds
        0xAA =>  0x2665, // hearts
        0xAB => 0x2660, // spades
        >=0xAC and <= 0xBC => input + 0x2780 - 0xAC,
        0xD5 => 0x2192,
        0XD6 => 0X2194,
        0XD7 => 0X2195,
        >= 0xA1 and < 0xF0 => input + 0x2761 - 0xA1,
        0XF0 => 0X27AF,
        0xFA=> 0x27BA,
        0xF9=> 0x2192,
        0xFF => 0x25A1,
        >= 0XF1 => input + 0x27b2 - 0xF1,
        _=>(0x2700 - 32 + input)
    });
}