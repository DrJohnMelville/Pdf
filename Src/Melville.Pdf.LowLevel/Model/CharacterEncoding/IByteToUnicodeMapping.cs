using System;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

public interface IByteToUnicodeMapping
{
    char MapToUnicode(byte input);
}

public class TableMapping : IByteToUnicodeMapping
{
    private readonly char[] mappings;
    public TableMapping(char[] mappings)
    {
        this.mappings = mappings;
        if (mappings.Length != 256) throw new ArgumentException("Wrong size for mapping array");
    }
    public char MapToUnicode(byte input) => mappings[input];
}

public class PassthroughMapping : IByteToUnicodeMapping
{
    public static readonly PassthroughMapping Instannce = new();

    private PassthroughMapping() { }

    public char MapToUnicode(byte input) => (char)input;
}