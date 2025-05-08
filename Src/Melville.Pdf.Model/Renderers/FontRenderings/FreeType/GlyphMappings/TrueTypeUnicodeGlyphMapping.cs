using System;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings
{
    internal class TrueTypeUnicodeGlyphMapping
        (IReadCharacter fontToUnicode, ICmapImplementation uncodeToGlyph) : IMapCharacterToGlyph
    {
#warning cmap should work on spans, which would let us save some allocations in here/

        /// <inheritdoc />
        public uint GetGlyph(uint character)
        {
            var source = new byte[] { (byte)(character >> 8), (byte)character };
            var buffer = new uint[4];
            fontToUnicode.GetCharacters(new ReadOnlyMemory<byte>(source), buffer.AsMemory(), out var _);
            return uncodeToGlyph.Map(buffer[0]);
        }
    }
}