using System;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.CharacterReaders;

public interface IReadCharacter
{
    (uint character, int bytesConsumed) GetNextChar(in ReadOnlySpan<byte> input);
}