using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

internal class CompositeMapper(IReadOnlyList<IMapCharacterToGlyph> mappers) : IMapCharacterToGlyph
{
    /// <inheritdoc />
    public uint GetGlyph(uint character)
    {
        foreach (var mapper in mappers)
        {
            if (mapper.GetGlyph(character) is var val and > 0) return val;
        }

        return 0;
    }
}

internal class CMapWrapper(ICmapImplementation inner) : IMapCharacterToGlyph
{
    /// <inheritdoc />
    public uint GetGlyph(uint character) => inner.Map(character);
}

internal readonly partial struct CharacterToGlyphMapFactory(IGenericFont iFont, PdfFont font, PdfEncoding encoding)
{
    public ValueTask<IMapCharacterToGlyph> ParseAsync() =>
        (font.SubType()) switch
        {
            var x when x.Equals(KnownNames.Type0) => Type0CharMappingAsync(),
            var x when x.Equals(KnownNames.MMType1) => SingleByteNamedMappingAsync(),
            var x when x.Equals(KnownNames.Type1) => SingleByteNamedMappingAsync(),
            var x when x.Equals(KnownNames.TrueType) =>  ParseTrueTypeMappingAsync(),
            _ => throw new PdfParseException("Unknown Font Type"),

        };

    // this is a variance from the spec.  If a symbolic true type font lacks a valid CMAP for mapping
    // we fall back and attempt to map the font as a roman font.
    private async ValueTask<IMapCharacterToGlyph> ParseTrueTypeMappingAsync() => 
        await TryMapAsSymbolicFontAsync(await font.FontFlagsAsync().CA()).CA() 
        ?? await SingleByteNamedMappingAsync().CA();

    private ValueTask<IMapCharacterToGlyph?> TryMapAsSymbolicFontAsync(FontFlags fontFlags) => 
        fontFlags.HasFlag(FontFlags.Symbolic) ? 
            TrueTypeSymbolicMappingAsync(): 
            ValueTask.FromResult<IMapCharacterToGlyph?>(null);

    private async ValueTask<IMapCharacterToGlyph?> TrueTypeSymbolicMappingAsync()
    {
        var source = await iFont.GetCmapSourceAsync().CA();
        var charmap =
            await source.GetByPlatformEncodingAsync(1, 0).CA() ??
            await source.GetByPlatformEncodingAsync(3, 0).CA();

        if (charmap is null) return null;
        return new CMapWrapper(charmap);
      }

    private async ValueTask<IMapCharacterToGlyph> SingleByteNamedMappingAsync()
    {
        var array = new uint[256];
        var nameToGlyphMapper = await new NameToGlyphMappingFactory(iFont).CreateAsync().CA();
        await new SingleByteEncodingParser(nameToGlyphMapper, array, 
                await BuiltInFontCharMappingsAsync().CA())
            .WriteEncodingToArrayAsync(encoding.LowLevel).CA();
        await WriteBackupMappingsAsync(array).CA();
        return new CharacterToGlyphArray(array);
    }

    private async ValueTask WriteBackupMappingsAsync(uint[] array)
    {
        // this is nonstandard behavior to handle a small population of PDF files that appear to be malformed.
        // the idea is to scan all of the CMaps and if there are any mappings from single bytes to a character that
        // is currently unmapped (ie mapped to glyph 0) then we use the backup mapping.  Since undefined mappings cause
        // undefined behavior, this should not affect any valid files, but it will allow some malformed files to
        // be presented correctly.

        var cmaps = await iFont.GetCmapSourceAsync().CA();

        for (int i = 0; i < cmaps.Count; i++)
        {
            foreach (var (_, character, glyph) in (await cmaps.GetByIndexAsync(i).CA()).AllMappings())
            {
                if (character < 256 && array[character] == 0)
                {
                    array[character] = glyph;
                }
            }
        }
    }

    private async Task<PdfDirectObject[]?> BuiltInFontCharMappingsAsync()
    {
        if (!font.SubType().Equals(KnownNames.Type1)) return null;
        return (await font.BaseFontNameAsync().CA()) switch
        {
            var x when x.Equals(KnownNames.Symbol) => CharacterEncodings.Symbol,
            var x when x.Equals(KnownNames.ZapfDingbats) => CharacterEncodings.ZapfDingbats,
            _=> null
        };
    }
    
    private async ValueTask<IMapCharacterToGlyph> Type0CharMappingAsync()
    {
        var subFont = await font.Type0SubFontAsync().CA();
        if (await subFont.CidToGidMapStreamAsync().CA() is { } mapStream)
            return await ParseCmapStreamAsync(mapStream).CA();
        
        var sysInfo = await subFont.CidSystemInfoAsync().CA();
        if (sysInfo is not null)
        {
            var ordering = await sysInfo.GetOrDefaultAsync(KnownNames.Ordering, KnownNames.Identity).CA();
            if (!ordering.Equals(KnownNames.Identity))
            {
                var cmaps = await iFont.GetCmapSourceAsync().CA();
                var list = new List<IMapCharacterToGlyph>();
                TryAdd(await cmaps.GetByPlatformEncodingAsync(3, 10).CA(), list);
                TryAdd(await cmaps.GetByPlatformEncodingAsync(3,1).CA(), list);
                TryAdd(await cmaps.GetByPlatformEncodingAsync(0, 4).CA(), list);
                TryAdd(await cmaps.GetByPlatformEncodingAsync(0, 3).CA(), list);
                for (int i = 0; i < cmaps.Count; i++)
                {
                    TryAdd(await cmaps.GetByIndexAsync(i).CA(), list);
                }
                return new CompositeMapper(list);
            }
        }


        return IdentityCharacterToGlyph.Instance;
    }

    private static void TryAdd(ICmapImplementation? cmap, List<IMapCharacterToGlyph> list)
    {
        if (cmap is not null)
        {
            list.Add(new CMapWrapper(cmap));
        }
    }

    private static async ValueTask<IMapCharacterToGlyph> ParseCmapStreamAsync(PdfStream mapStream)
    {
        await using var stream = await mapStream.StreamContentAsync().CA();
        return await new CMapStreamParser(
            PipeReader.Create(stream)).ParseAsync().CA();
    }
}