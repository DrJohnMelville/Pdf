using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
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
    public static int ReqiredBufferSize(this IPdfBitmap bitmap) => 4 * TotalPixels(bitmap);
    public static int TotalPixels(this IPdfBitmap bitmap) => bitmap.Width * bitmap.Height;

    public static async ValueTask<IPdfBitmap> WrapForRenderingAsync(this PdfStream stream) =>
        new PdfBitmapWrapper(PipeReader.Create(await stream.StreamContentAsync()),
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1)
        );
}

public class PdfBitmapWrapper : IPdfBitmap
{
    public int Width { get; }
    public int Height { get; }
    private readonly PipeReader source;

    public PdfBitmapWrapper(PipeReader source, int width, int height)
    {
        this.source = source;
        Width = width;
        Height = height;
    }

    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        var x = new BitmapWriter(buffer,source, this.TotalPixels());
        return InnerRender(x);
    }

    private async ValueTask InnerRender(BitmapWriter c)
    {
        int row = Height - 1;
        int column = 0;
        while (c.LoadLPixels(await c.reader.ReadAsync(), ref row, ref column)) {/* do nothing*/}

    }
}

unsafe readonly struct BitmapWriter
{
    private readonly byte* buffer;
    public readonly PipeReader reader;
    public readonly int totalPixels;
    private const int BytesPerPixel = 4; 
    
    public BitmapWriter(byte* buffer, PipeReader reader, int totalPixels)
    {
        this.buffer = buffer;
        this.reader = reader;
        this.totalPixels = totalPixels;
    }


    public bool LoadLPixels(ReadResult readResult, ref int row, ref int col)
    {
        if (readResult.IsCompleted && readResult.Buffer.Length < BytesPerPixel) return false;
        var localPointer = buffer + (currentPixel * 4);
        
        var seq = new SequenceReader<byte>(readResult.Buffer);
        for (int i = 0; i < remaining; i++)
        {
            if (seq.Remaining < BytesPerPixel)
            {
                reader.AdvanceTo(seq.Position, readResult.Buffer.End);
                return i;
            }

            seq.TryRead(out *(localPointer+2));
            seq.TryRead(out *(localPointer+1));
            seq.TryRead(out *localPointer);
            localPointer += 3;            
            *localPointer++ = (byte)0xff;
        }

        var TSpan = new Span<byte>(buffer, 4 * totalPixels);
        return remaining;
    }
}