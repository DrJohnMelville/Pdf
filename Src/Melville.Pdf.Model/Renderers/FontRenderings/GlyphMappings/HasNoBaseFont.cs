using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings
{
    [StaticSingleton]
    internal partial class HasNoBaseFont: IReadCharacter
    {
        public Span<uint> GetCharacters(
            in ReadOnlySpan<byte> input, in Span<uint> scratchBuffer, out int bytesConsumed) => 
            throw new PdfParseException("Builtin CMAPS should not rely on a base font.");
    }
}