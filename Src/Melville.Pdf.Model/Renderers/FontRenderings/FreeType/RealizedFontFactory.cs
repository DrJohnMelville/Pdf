using System.IO;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal readonly partial struct RealizedFontFactory
{
    [FromConstructor] private readonly PdfFont fontDefinitionDictionary;

    public async ValueTask<IRealizedFont> FromStreamAsync(PdfStream pdfStream)
    {
        var source = await pdfStream.StreamContentAsync().CA();
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
        var font = await RootFontParser.ParseAsync(fontSource).CA();
        return font[index];
    }

    private async ValueTask<IRealizedFont> FontFromFaceAsync(IGenericFont iFace)
    {
        var encoding = await fontDefinitionDictionary.EncodingAsync().CA();
        return
            new GenericToRealizedFontWrapper(iFace,
                await new FontRenderings.GlyphMappings.ReadCharacterFactory(
                        fontDefinitionDictionary, encoding,
                        await new NameToGlyphMappingFactory(iFace).CreateAsync().CA())
                    .CreateAsync().CA(), 
                await new CharacterToGlyphMapFactory(iFace, fontDefinitionDictionary, encoding).ParseAsync().CA(), 
                await new FontWidthParser(fontDefinitionDictionary).ParseAsync().CA());
    }
}