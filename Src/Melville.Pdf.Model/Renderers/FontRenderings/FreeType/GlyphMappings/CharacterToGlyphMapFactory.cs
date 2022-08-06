using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

public readonly partial struct CharacterToGlyphMapFactory
{
    [FromConstructor] private readonly Face face;
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfObject? encoding;

    public async ValueTask<IMapCharacterToGlyph> Parse() =>
        (await font.SubTypeAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Type0 => throw new NotImplementedException("Type 0 character mapping not implemented"),
            KnownNameKeys.Type1 => throw new NotImplementedException("Type 1 character mapping not implemented"),
            KnownNameKeys.TrueType => await ParseTrueTypeMapping(),
            _ => throw new PdfParseException("Unknown Font Type"),

        };

    private async Task<IMapCharacterToGlyph> ParseTrueTypeMapping()
    {
        var symbolic = (await font.FontFlagsAsync().CA()).HasFlag(FontFlags.Symbolic);
        uint[] mapping = symbolic ? await TrueTypeSymbolicMapping():
            throw new NotImplementedException("NonSymbolic font mapping");
        return new CharacterToGlyphArray(mapping);
    }

    private async Task<uint[]> TrueTypeSymbolicMapping()
    {
        var ret = new uint[256];
        var charmap = face.CharMapByInts(1, 0) ?? face.CharMapByInts(3, 0);
        if (charmap is not null)
        {
            foreach (var (character, glyph) in charmap.AllMappings())
            {
                ret[character & 0xFF] = glyph;
            }
        }

        return ret;
    }
}
