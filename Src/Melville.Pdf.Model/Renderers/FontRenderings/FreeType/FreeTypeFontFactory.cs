using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public class FreeTypeFontFactory
{
    private static readonly Library sharpFontLibrary = new Library();
    public static ValueTask<IRealizedFont> SystemFont(byte[] name, double size, IFontTarget target,
        IByteToUnicodeMapping mapping, bool bold, bool oblique)
    {
        if (name == null) throw new InvalidProgramException();
        return FromCSharpStream(size, target, mapping, File.OpenRead("c:/Windows/Fonts/Cour.ttf"));
    }
    
    public static async ValueTask<IRealizedFont> FromStream(PdfStream pdfStream, double size, IFontTarget target,
        IByteToUnicodeMapping mapping)
    {
        var source = await pdfStream.StreamContentAsync();
        return await FromCSharpStream(size, target, mapping, source);
    }

    private static async ValueTask<IRealizedFont> FromCSharpStream(double size, IFontTarget target, IByteToUnicodeMapping mapping,
        Stream source)
    {
        var face = sharpFontLibrary.NewMemoryFace(await UncompressToBufferAsync(source), 0);
        face.SetCharSize(0, 64 * size, 96, 96);
        return new FreeTypeFont(face, target, mapping, size);
    }

    private static async Task<byte[]> UncompressToBufferAsync(Stream source)
    {
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource);
        var output = new byte[decodedSource.Length];
        await output.FillBufferAsync(0, output.Length, decodedSource.CreateReader());
        return output;
    }
}