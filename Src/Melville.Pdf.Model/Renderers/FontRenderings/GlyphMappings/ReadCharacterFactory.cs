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
            (await ReadCMapAsync(encoding.LowLevel, HasNoBaseFont.Instance).CA())??
            TwoByteCharacters.Instance;

       return outerFontCMap;
    }

    private ValueTask<IReadCharacter?> ReadCMapAsync(
        PdfDirectObject cMapName, IReadCharacter baseMapper) =>
        new CMapFactory(nameMapper,baseMapper, BuiltinCmapLibrary.Instance)
            .ParseCMapAsync(cMapName);
}