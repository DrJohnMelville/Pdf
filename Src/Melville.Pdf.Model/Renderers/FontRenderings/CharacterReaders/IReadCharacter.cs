using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

internal interface IReadCharacter
{
    (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input);
}