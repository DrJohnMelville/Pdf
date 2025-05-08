using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ParserMapping;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal readonly partial struct RealizedFontFactory
{
    [FromConstructor] private readonly PdfFont fontDefinitionDictionary;

    public async ValueTask<IRealizedFont> FromStreamAsync(PdfStream pdfStream, ParseMap? map)
    {
        await map.SetDataAsync(pdfStream).CA();
        var source = await pdfStream.StreamContentAsync().CA();
        map?.AddAlias(source);
        return await FromCSharpStreamAsync(source).CA();
    }
    
    public async ValueTask<IRealizedFont> FromCSharpStreamAsync(
        Stream source, int index = 0)
    {
        var iFace = await StreamToGenericFontMFAsync(source, index).CA();
        return await FontFromFaceAsync(iFace).CA();
    }

    private static async Task<IGenericFont> StreamToGenericFontMFAsync(Stream source, int index)
    {
        var fontSource = MultiplexSourceFactory.Create(source);
        source.AddParseMapAlias(fontSource);
        var font = await RootFontParser.ParseAsync(fontSource).CA();
        return font[index];
    }

    private async ValueTask<IRealizedFont> FontFromFaceAsync(IGenericFont iFace)
    {
        var encoding = await fontDefinitionDictionary.EncodingAsync().CA();
        var characterToGlyph = await new CharacterToGlyphMapFactory(iFace, fontDefinitionDictionary, encoding).ParseAsync().CA();
        var nameToGlyphMapping = await new NameToGlyphMappingFactory(iFace).CreateAsync().CA();
        var readCharacter = await new ReadCharacterFactory(fontDefinitionDictionary, encoding, nameToGlyphMapping).CreateAsync().CA();
        var fontWidthComputer = await new FontWidthParser(fontDefinitionDictionary).ParseAsync().CA();
        return
            new GenericToRealizedFontWrapper(iFace,
                readCharacter, 
                characterToGlyph, 
                fontWidthComputer);
    }
}