using Melville.Pdf.LowLevel.Model.CharacterEncoding;

namespace Melville.Pdf.Model.FontMappings;

public class NullUnicodeMapping : IByteToUnicodeMapping
{
    public static readonly NullUnicodeMapping Instance = new();

    private NullUnicodeMapping()
    {
    }

    public char MapToUnicode(byte input) => (char)input;
}