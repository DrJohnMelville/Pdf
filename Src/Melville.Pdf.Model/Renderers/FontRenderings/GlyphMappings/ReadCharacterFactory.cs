using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings.BuiltInCMaps;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal readonly partial struct ReadCharacterFactory
{
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;
    [FromConstructor] private readonly INameToGlyphMapping nameMapper;

    public  ValueTask<IReadCharacter> CreateAsync() =>
        KnownNames.Type0.Equals(font.SubType()) ?
            ParseType0FontEncodingAsync(): 
            new(SingleByteCharacters.Instance);

    private async ValueTask<IReadCharacter> ParseType0FontEncodingAsync()
    {
       var outerFontCMap = encoding.IsIdentityCdiEncoding()?
            TwoByteCharacters.Instance:
            (await ReadCMapAsync(encoding.LowLevel, HasNoBaseFont.Instance).CA())??SingleByteCharacters.Instance;
       
        var inner = await font.Type0SubFontAsync().CA();
        var sysInfo = await inner.CidSystemInfoAsync().CA();
        if (sysInfo is null) return outerFontCMap;

        return (await InnerFontOrderingCMapAsync().CA() is { } innerCmapName ?
            await ConstructCompositeAsync(innerCmapName, outerFontCMap).CA() : null)??
            await FailedCmapReadAsync(outerFontCMap).CA();
    }

    private async Task<IReadCharacter> FailedCmapReadAsync(IReadCharacter outerFontCMap)
    {
        if (await font.ToUnicode() is not { IsNull: false } unicode) return outerFontCMap;
        return await 
            ReadUnicodeCmapAsync(unicode).CA() ?? outerFontCMap;
    }

    private static ValueTask<IReadCharacter?> ReadUnicodeCmapAsync(PdfDirectObject unicode)
    {
        return new CMapFactory(GlyphNameToUnicodeMap.AdobeGlyphList, TwoByteCharacters.Instance)
            .ParseCMapAsync(unicode);
    }

    private async Task<IReadCharacter?> ConstructCompositeAsync(
        PdfDirectObject innerCMapName, IReadCharacter outerFontCMap) =>
        await ReadCMapAsync(innerCMapName, TwoByteCharacters.Instance).CA() 
               is {} inner? 
            new CompositeCmap(outerFontCMap, inner): 
            await FailedCmapReadAsync(outerFontCMap).CA();

    private async ValueTask<PdfDirectObject?> InnerFontOrderingCMapAsync()
    {
        var inner = await font.Type0SubFontAsync().CA();
        var sysInfo = await inner.CidSystemInfoAsync().CA();
        if (sysInfo is null) return null;

        var ordering = await sysInfo.GetOrDefaultAsync(KnownNames.Ordering, KnownNames.Identity).CA();
        var registry = await sysInfo.GetOrDefaultAsync(KnownNames.Registry, KnownNames.Identity).CA();
        if (registry.Equals(KnownNames.Identity) || ordering.Equals(KnownNames.Identity))
            return null;

        return PdfDirectObject.CreateName($"{registry}-{ordering}-UCS2");
    }

    private ValueTask<IReadCharacter?> ReadCMapAsync(
        PdfDirectObject cMapName, IReadCharacter baseMapper) =>
        new CMapFactory(nameMapper,baseMapper, BuiltinCmapLibrary.Instance)
            .ParseCMapAsync(cMapName);
}