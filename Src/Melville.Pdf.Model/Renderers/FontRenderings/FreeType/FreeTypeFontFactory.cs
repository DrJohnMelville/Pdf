using System.IO;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Hacks;
using Melville.Hacks.Reflection;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

internal readonly partial struct FreeTypeFontFactory
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
        var iFace = await StreamToGenericFontMF(source, index).CA();
        return await FontFromFaceAsync(iFace).CA();
    }

    private static async Task<IGenericFont> StreamToGenericFontMF(Stream source, int index) =>
        (await RootFontParser.ParseAsync(MultiplexSourceFactory.Create(source)).CA())[index];

    private static async Task<IGenericFont> StreamToGenericFontFT(Stream source, int index)
    {
        var fontAsBytes = await UncompressToBufferAsync(source).CA();
        await source.DisposeAsync().CA();
        await GlobalFreeTypeMutex.WaitForAsync().CA();
        Face face;
        try
        {
            face = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(fontAsBytes, index);
        }
        finally
        {
            GlobalFreeTypeMutex.Release();
        }
        face.SetCharSize(0, 65, 0, 0);
        var iFace = new FreeTypeFace(face);
        return iFace;
    }

    private static async Task<byte[]> UncompressToBufferAsync(Stream source)
    {
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource).CA();
        var output = new byte[decodedSource.Length]; // We cannot rent this because Face keeps the reference.
        await output.FillBufferAsync(0, output.Length, decodedSource.CreateReader()).CA();
        return output;
    }
    private async ValueTask<IRealizedFont> FontFromFaceAsync(IGenericFont iFace)
    {
        var encoding = await fontDefinitionDictionary.EncodingAsync().CA();
        return
            new FreeTypeFont(iFace,
                await new FontRenderings.GlyphMappings.ReadCharacterFactory(
                        fontDefinitionDictionary, encoding,
                        await new NameToGlyphMappingFactory(iFace).CreateAsync().CA())
                    .CreateAsync().CA(), 
                await new CharacterToGlyphMapFactory(iFace, fontDefinitionDictionary, encoding).ParseAsync().CA(), 
                await new FontWidthParser(fontDefinitionDictionary).ParseAsync().CA());
    }
}