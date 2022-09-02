using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.FontWidths;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly partial struct FreeTypeFontFactory
{
    [FromConstructor] private readonly double size;
    [FromConstructor] private readonly PdfFont fontDefinitionDictionary;

    public async ValueTask<IRealizedFont> FromStream(PdfStream pdfStream)
    {
        await using var source = await pdfStream.StreamContentAsync().CA();
        return await FromCSharpStream(source).CA();
    }

    public async ValueTask<IRealizedFont> FromCSharpStream(Stream source, int index = 0)
    {
        var face = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(
            await UncompressToBufferAsync(source).CA(), index);
        return await FontFromFace(face).CA();
    }

    private static async Task<byte[]> UncompressToBufferAsync(Stream source)
    {
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource).CA();
        var output = new byte[decodedSource.Length];
        await output.FillBufferAsync(0, output.Length, decodedSource.CreateReader()).CA();
        return output;
    }

    private async ValueTask<IRealizedFont> FontFromFace(Face face)
    {
        face.SetCharSize(0, 64 * size, 0, 0);
        var encoding = await fontDefinitionDictionary.EncodingAsync().CA();
        return new FreeTypeFont(face, 
            await new CharacterReaders.ReadCharacterFactory(fontDefinitionDictionary, encoding).Create().CA(), 
            await new CharacterToGlyphMapFactory(face, fontDefinitionDictionary,
                encoding).Parse().CA(), 
            await new FontWidthParser(fontDefinitionDictionary, size).Parse().CA());
    }
}