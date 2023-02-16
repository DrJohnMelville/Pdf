using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

[StaticSingleton]
internal sealed partial class TwoByteCharacters : IReadCharacter
{
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => 
        ((uint)(input[0] << 8)|input[1], 2);
}
