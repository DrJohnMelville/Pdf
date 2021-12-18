using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

public interface IPdfBitmap
{
    int Width { get; }
    int Height { get; }
    /// <summary>
    /// Fill the bitmap pointed to by buffer.
    /// This is implemented as an unsafe pointer operation so that I can quickly fill native buffers,
    /// deep in the graphics stack.
    /// </summary>
    /// <param name="buffer">A byte pointer which must point to the beginning of a buffer that
    /// is Width * Height *4 bytes long</param>
    unsafe ValueTask RenderPbgra(byte* buffer);
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
    
    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        for (int i = 0; i < this.ReqiredBufferSize(); i++)
        {
            *buffer++ = (byte)(i %4 != 2 ? 0xFF:0);
        }
        return ValueTask.CompletedTask;
    }
}