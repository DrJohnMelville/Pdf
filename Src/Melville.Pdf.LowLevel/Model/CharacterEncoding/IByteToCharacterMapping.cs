using System;

namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;

public interface IByteToCharacterMapping
{
    uint MapToUnicode(byte input);
}

public class TableMapping : IByteToCharacterMapping
{
    private readonly uint[] mappings;
    public TableMapping(uint[] mappings)
    {
        this.mappings = mappings;
        if (mappings.Length != 256) throw new ArgumentException("Wrong size for mapping array");
    }
    public uint MapToUnicode(byte input) => mappings[input];
}

public class PassthroughMapping : IByteToCharacterMapping
{
    public static readonly PassthroughMapping Instannce = new();

    private PassthroughMapping() { }

    public uint MapToUnicode(byte input) => input;
}