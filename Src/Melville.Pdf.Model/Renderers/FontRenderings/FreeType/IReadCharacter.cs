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

public sealed class TwoByteCharacters : IReadCharacter
{
    public static TwoByteCharacters Instance = new();
    private TwoByteCharacters() { }
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => 
        ((uint)(input[0] << 8)|input[1], 2);
}
