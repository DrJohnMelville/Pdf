using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IPdfBitmap
{
    int Width { get; }
    int Height { get; }
    ValueTask RenderPbgra(Memory<byte> buffer);
}

public static class PdfBitmapOperatons
{
    public static int ReqiredBufferSize(this IPdfBitmap bitmap) => 4 * bitmap.Width * bitmap.Height;

    public static async ValueTask<IPdfBitmap> WrapForRenderingAsync(this PdfStream stream) =>
        new PdfBitmapWrapper(stream,
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1)
        );
}

public class PdfBitmapWrapper: IPdfBitmap
{
    public int Width { get; }
    public int Height { get; }
    private readonly PdfStream source;

    public PdfBitmapWrapper(PdfStream source, int width, int height)
    {
        this.source = source;
        Width = width;
        Height = height;
    }
    
    public ValueTask RenderPbgra(Memory<byte> buffer)
    {
        var pixels = buffer.Span;
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (byte)(i %4 != 2 ? 0xFF:0);
        }

        return ValueTask.CompletedTask;
    }
}