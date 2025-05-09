using System;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings
{
    internal class TrueTypeUnicodeGlyphMapping
        (IReadCharacter fontToUnicode, ICmapImplementation uncodeToGlyph) : IMapCharacterToGlyph
    {
        /// <inheritdoc />
        public uint GetGlyph(uint character)
        {
            Span<byte> source = [(byte)(character >> 8), (byte)character];
            Span<uint> buffer = stackalloc uint[4];
            fontToUnicode.GetCharacters(source, buffer, out var _);
            return uncodeToGlyph.Map(buffer[0]);
        }
    }
}