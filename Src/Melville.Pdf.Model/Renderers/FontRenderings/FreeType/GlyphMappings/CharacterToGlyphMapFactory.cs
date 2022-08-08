using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
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
            KnownNameKeys.Type0 => await Type0CharMapping().CA(),
            KnownNameKeys.MMType1 => throw new NotImplementedException("MultiMaster fonts not implemented."),
            KnownNameKeys.Type1 => await SingleByteNamedMapping().CA(),
            KnownNameKeys.TrueType => await ParseTrueTypeMapping().CA(),
            _ => throw new PdfParseException("Unknown Font Type"),

        };

    private async ValueTask<IMapCharacterToGlyph> ParseTrueTypeMapping()
    {
        var symbolic = (await font.FontFlagsAsync().CA()).HasFlag(FontFlags.Symbolic);
        return symbolic ? 
            new CharacterToGlyphArray(await TrueTypeSymbolicMapping().CA()):
            await SingleByteNamedMapping().CA();
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
    
    private async ValueTask<IMapCharacterToGlyph> SingleByteNamedMapping()
    {
        var array = new uint[256];
        var nameToGlyphMapper = new NameToGlyphMappingFactory(face).Create();
        await new SingleByteEncodingParser(nameToGlyphMapper, array, await BuiltInFontCharMappings().CA())
            .WriteEncodingToArray(encoding).CA();
        return new CharacterToGlyphArray(array);
    }

    private async Task<byte[][]?> BuiltInFontCharMappings()
    {
        if ((await font.SubTypeAsync().CA()) != KnownNames.Type1) return null;
        return (await font.BaseFontNameAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Symbol => CharacterEncodings.Symbol,
            KnownNameKeys.ZapfDingbats => CharacterEncodings.ZapfDingbats,
            _=> null
        };
    }
    
    private async ValueTask<IMapCharacterToGlyph> Type0CharMapping()
    {
        return IdentityCharacterToGlyph.Instance;
    }


}