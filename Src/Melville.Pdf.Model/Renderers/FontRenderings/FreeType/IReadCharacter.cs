using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public interface IReadCharacter
{
    (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input);
}

public sealed class SingleByteCharacters : IReadCharacter
{
    public static SingleByteCharacters Instance = new();
    private SingleByteCharacters() { }
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => (input[0], 1);
}