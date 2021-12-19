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
        var bitsPerComponent = (int)await stream.GetOrDefaultAsync(KnownNames.BitsPerComponent, 8);
        return CreateByteWriter(colorSpace, bitsPerComponent);
    }

    private static IByteWriter CreateByteWriter(IColorSpace colorSpace, int bitsPerComponent) =>
        (colorSpace, bitsPerComponent) switch
        {
            (_, 16) => new ByteWriter16(colorSpace),
            (IndexedColorSpace, _) => new IntegerComponentByteWriter(colorSpace, bitsPerComponent),
            (DeviceRgb, 8) => new FastBitmapWriterRGB8(),
            _ => new NBitByteWriter(colorSpace, bitsPerComponent)
        };
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
        if (readResult.IsCompleted && !EnoughBytesToRead(readResult.Buffer.Length))
            return false;
        var seq = new SequenceReader<byte>(readResult.Buffer);
        while (EnoughBytesToRead(seq.Remaining))
        {
            byte* localPointer = buffer + PixelOffset(row, col);
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

        reader.AdvanceTo(seq.Position, readResult.Buffer.End);
        return row >= 0;
    }

    private bool EnoughBytesToRead(long bytesRemaining) => 
        bytesRemaining >= writer.MinimumInputSize;

    private int PixelOffset(int row, int col) => 4 * PixelPosition(row, col);
    private int PixelPosition(int row, int col) => (col + (row * width));
}