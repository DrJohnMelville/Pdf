using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly struct FreeTypeFontFactory
{
    private readonly double size;
    public IByteToUnicodeMapping? ByteToUnicodeMapping { get; init; } = null;
    public IGlyphMapping? GlyphMapping { get; init; } = null;
    private readonly PdfFont font;

    public FreeTypeFontFactory(double size, in PdfFont font)
    {
        this.size = size;
        this.font = font;
    }

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
        return new FreeTypeFont(face, GlyphMapping ?? await CreateGlyphMap(face).CA());
    }

    private async ValueTask<IGlyphMapping> CreateGlyphMap(Face face) =>
        (await font.FontFlagsAsync().CA()).HasFlag(FontFlags.Symbolic)
            ? await
                SymbolicEncodingParser.ParseGlyphMapping(face, await font.EncodingAsync().CA()).CA()
            : await RomanGlyphMapping(face).CA();

    private async ValueTask<IGlyphMapping> RomanGlyphMapping(Face face)
    {
        var encoding = await font.EncodingAsync().CA();
        return new UnicodeGlyphMapping(face,
            ByteToUnicodeMapping ?? await RomanEncodingParser.InterpretEncodingValue(encoding).CA());
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