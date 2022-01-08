using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.FontMappings;

public interface IByteToUnicodeMapping
{
    char MapToUnicode(byte input);
}

public class DefaultUnicodeMapping : IByteToUnicodeMapping
{
    public static readonly DefaultUnicodeMapping Instance = new();
    private DefaultUnicodeMapping()
    {
    }

    public char MapToUnicode(byte input) => (char)input;
}

public interface IFontMapping
{
    object Font { get; }
    IByteToUnicodeMapping Mapping { get; }
    bool Bold { get; }
    bool Oblique { get; }
}

public interface IDefaultFontMapper
{
    public IFontMapping MapDefaultFont(PdfName font);
}