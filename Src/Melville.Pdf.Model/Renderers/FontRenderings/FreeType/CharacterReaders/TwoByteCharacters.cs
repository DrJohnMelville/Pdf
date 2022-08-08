using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.CharacterReaders;

public sealed class TwoByteCharacters : IReadCharacter
{
    public static TwoByteCharacters Instance = new();
    private TwoByteCharacters() { }
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => 
        ((uint)(input[0] << 8)|input[1], 2);
}
