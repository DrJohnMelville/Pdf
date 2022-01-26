using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

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
        while (c.LoadLPixels(await c.ReadAsync().ConfigureAwait(false), ref row, ref column))
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