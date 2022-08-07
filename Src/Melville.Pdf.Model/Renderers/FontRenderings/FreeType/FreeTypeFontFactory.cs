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

    public ValueTask<IRealizedFont> SystemFont(byte[] name, bool bold, bool oblique)
    {
        var fontRef = GlobalFreeTypeResources.SystemFontLibrary().FontFromName(name, bold, oblique);
        var face = GlobalFreeTypeResources.SharpFontLibrary.NewFace(fontRef.FileName, fontRef.Index);
        return FontFromFace(face);
    }
    
    public async ValueTask<IRealizedFont> FromStream(PdfStream pdfStream)
    {
        var source = await pdfStream.StreamContentAsync().CA();
        return await FromCSharpStream(source).CA();
    }

    private async ValueTask<IRealizedFont> FromCSharpStream(Stream source)
    {
        var face = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(
            await UncompressToBufferAsync(source).CA(), 0);
        return await FontFromFace(face).CA();
    }

    private async ValueTask<IRealizedFont> FontFromFace(Face face)
    {
        face.SetCharSize(0, 64 * size, 0, 0);
        return new FreeTypeFont(face, SingleByteCharacters.Instance, 
            await new CharacterToGlyphMapFactory(face, fontDefinitionDictionary,
                await fontDefinitionDictionary.EncodingAsync().CA()).Parse().CA(), 
            await new FontWidthParser(fontDefinitionDictionary, size).Parse().CA());
    }
    
    private static async Task<byte[]> UncompressToBufferAsync(Stream source)
    {
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource).CA();
        var output = new byte[decodedSource.Length];
        await output.FillBufferAsync(0, output.Length, decodedSource.CreateReader()).CA();
        return output;
    }
}