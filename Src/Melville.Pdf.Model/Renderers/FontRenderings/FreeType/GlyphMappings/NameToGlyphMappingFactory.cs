using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal readonly struct NameToGlyphMappingFactory(IGenericFont font)
{
    public async ValueTask<INameToGlyphMapping> CreateAsync() =>
        new CompositeGlyphNameMapper(
            await NamesFromFaceAsync().CA(),
            await UnicodeAndAdobeGlyphListAsync().CA(),
            await UnicodeViaMacGlyphListAsync().CA()
        );

    private async ValueTask<INameToGlyphMapping?> NamesFromFaceAsync()
    {
        var names = await font.GlyphNamesAsync().CA();
        if (names.Length == 0) return null;
        var dictionary = new Dictionary<uint, uint>(names.Length);

        for (uint i = 0; i < names.Length; i++)
        {
            dictionary[FnvHash.FnvHashAsUInt(names[i])] = i;
        }
        
        return new DictionaryGlyphNameMapper(dictionary);
    }

    private async ValueTask<INameToGlyphMapping?> UnicodeAndAdobeGlyphListAsync()
    {
        var source = await font.GetCmapSourceAsync().CA();
        var cmap = await source.GetUnicodeCMapAsync().CA();
        return cmap != null ? 
            new UnicodeGlyphNameMapper(MappingToDictionary(cmap)) : null;
    }

    private static Dictionary<uint, uint> MappingToDictionary(CharMap mapping) => 
        mapping.AllMappings().ToDictionary(i => i.Char, i => i.Glyph);

    private static Dictionary<uint, uint> MappingToDictionary(ICmapImplementation mapping) => 
        mapping.AllMappings().ToDictionary(i => i.Character, i => i.Glyph);

    private async ValueTask<INameToGlyphMapping?> UnicodeViaMacGlyphListAsync()
    {
        var m2 = await (await font.GetCmapSourceAsync().CA())
            .GetByPlatformEncodingAsync(1, 0).CA();
        if (m2 is null) return null;
        return new UnicodeViaMacMapper(MappingToDictionary(m2));
    }
}