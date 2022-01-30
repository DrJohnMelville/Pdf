using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFontFactory
{
    private static readonly Library sharpFontLibrary = new Library();
    
    private static FontLibrary? instance;

    public static FontLibrary Instance() =>
        instance ?? SetFontDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

    public static FontLibrary SetFontDirectory(string fontFolder)
    {
        instance = new FontLibraryBuilder(sharpFontLibrary).BuildFrom(fontFolder);
        return instance;
    }

    public static ValueTask<IRealizedFont> SystemFont(byte[] name, double size,
        IByteToUnicodeMapping? mapping, bool bold, bool oblique)
    {
        var fontRef = Instance().FontFromName(name, bold, oblique);
        var face = sharpFontLibrary.NewFace(fontRef.FileName, fontRef.Index);
        return new(FontFromFace(size, mapping, face));
    }
    
    public static async ValueTask<IRealizedFont> FromStream(PdfStream pdfStream, double size,
        IByteToUnicodeMapping? mapping)
    {
        var source = await pdfStream.StreamContentAsync().ConfigureAwait(false);
        return await FromCSharpStream(size, mapping, source).ConfigureAwait(false);
    }

    private static async ValueTask<IRealizedFont> FromCSharpStream(double size, IByteToUnicodeMapping? mapping,
        Stream source)
    {
        var face = sharpFontLibrary.NewMemoryFace(await UncompressToBufferAsync(source).ConfigureAwait(false), 0);
        return FontFromFace(size, mapping, face);
    }

    private static IRealizedFont FontFromFace(double size, IByteToUnicodeMapping? mapping, Face face)
    {
        face.SetCharSize(0, 64 * size, 0, 0);
        return new FreeTypeFont(face, mapping);
    }

    private static async Task<byte[]> UncompressToBufferAsync(Stream source)
    {
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource).ConfigureAwait(false);
        var output = new byte[decodedSource.Length];
        await output.FillBufferAsync(0, output.Length, decodedSource.CreateReader()).ConfigureAwait(false);
        return output;
    }
}