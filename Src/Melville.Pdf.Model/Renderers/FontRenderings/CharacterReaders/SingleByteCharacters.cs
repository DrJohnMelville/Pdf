using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

[StaticSingleton()]
internal sealed partial class SingleByteCharacters : IReadCharacter
{
    public (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input) => (input[0], 1);
}