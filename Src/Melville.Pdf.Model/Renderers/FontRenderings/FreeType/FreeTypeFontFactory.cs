using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public readonly struct FreeTypeFontFactory
{
    private readonly double size;
    public IByteToUnicodeMapping Mapping { get; init; }
    private readonly IFontWidthComputer? widthComputer;

    public FreeTypeFontFactory(double size, IByteToUnicodeMapping? mapping, IFontWidthComputer? widthComputer)
    {
        this.size = size;
        this.Mapping = mapping;
        this.widthComputer = widthComputer;
    }

    public ValueTask<IRealizedFont> SystemFont(byte[] name, bool bold, bool oblique)
    {
        var fontRef = GlobalFreeTypeResources.SystemFontLibrary().FontFromName(name, bold, oblique);
        var face = GlobalFreeTypeResources.SharpFontLibrary.NewFace(fontRef.FileName, fontRef.Index);
        return new(FontFromFace(face));
    }
    
    public async ValueTask<IRealizedFont> FromStream(PdfStream pdfStream)
    {
        var source = await pdfStream.StreamContentAsync().CA();
        return await FromCSharpStream(source).CA();
    }

    private async ValueTask<IRealizedFont> FromCSharpStream(Stream source)
    {
        var face = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(await UncompressToBufferAsync(source).CA(), 0);
        return FontFromFace(face);
    }

    private IRealizedFont FontFromFace(Face face)
    {
        face.SetCharSize(0, 64 * size, 0, 0);
        return new FreeTypeFont(face, Mapping, widthComputer);
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