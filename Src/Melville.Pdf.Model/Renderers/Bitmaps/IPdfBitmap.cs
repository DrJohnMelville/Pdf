using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;

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

    public static async ValueTask<IPdfBitmap> WrapForRenderingAsync(
        this PdfStream stream, PdfPage page) =>
        new PdfBitmapWrapper(PipeReader.Create(await stream.StreamContentAsync()),
            (int)await stream.GetOrDefaultAsync(KnownNames.Width, 1),
            (int)await stream.GetOrDefaultAsync(KnownNames.Height, 1),
            await GetByteWriterAsync(stream, page)
        );

    private static async ValueTask<IByteWriter> GetByteWriterAsync(
        PdfStream stream, PdfPage page)
    {
        var colorSpace = await ColorSpaceFactory.FromNameOrArray(
            await stream[KnownNames.ColorSpace], page);
        return (int)await stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8) switch
        {
            16 => new ByteWriter16(colorSpace),
            var bits => new NBitByteWriter(colorSpace, bits)
        };
    }
}

public class PdfBitmapWrapper : IPdfBitmap
{
    public int Width { get; }
    public int Height { get; }
    private readonly IByteWriter byteWriter;
    private readonly PipeReader source;

    public PdfBitmapWrapper(PipeReader source, int width, int height, IByteWriter byteWriter)
    {
        this.source = source;
        Width = width;
        Height = height;
        this.byteWriter = byteWriter;
    }

    public unsafe ValueTask RenderPbgra(byte* buffer)
    {
        var x = new BitmapWriter(buffer, source, Width, byteWriter);
        return InnerRender(x);
    }

    private async ValueTask InnerRender(BitmapWriter c)
    {
        int row = Height - 1;
        int column = 0;
        while (c.LoadLPixels(await c.ReadAsync(), ref row, ref column))
        {
            /* do nothing*/
        }
    }
}

unsafe readonly struct BitmapWriter
{
    private readonly byte* buffer;
    private readonly PipeReader reader;
    private readonly int width;
    private readonly IByteWriter writer;

    public BitmapWriter(byte* buffer, PipeReader reader, int width, IByteWriter writer)
    {
        this.buffer = buffer;
        this.reader = reader;
        this.width = width;
        this.writer = writer;
    }

    public ValueTask<ReadResult> ReadAsync() => reader.ReadAsync();

    public bool LoadLPixels(ReadResult readResult, ref int row, ref int col)
    {
        if (readResult.IsCompleted && readResult.Buffer.Length == 0)
            return false;
        var seq = new SequenceReader<byte>(readResult.Buffer);
        while (seq.Remaining > 0)
        {
            byte* localPointer = buffer + 4 * (col + (row * width));
            byte* oneOffEnd = localPointer + ((width - col) * 4);
            writer.WriteBytes(ref seq, ref localPointer, oneOffEnd);
            if (oneOffEnd == localPointer)
            {
                row--;
                col = 0;
            }
            else
            {
                col = width - (int)(oneOffEnd - localPointer) / 4;
            }
        }

        reader.AdvanceTo(seq.Position);
        return row >= 0;
    }
}