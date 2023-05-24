using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class PdfBitmapWrapper : IPdfBitmap
{
    public int Width { get; }
    public int Height { get; }
    public bool DeclaredWithInterpolation { get; }
    private readonly IByteWriter byteWriter;
    private readonly PipeReader source;

    public PdfBitmapWrapper(
        PipeReader source, int width, int height, bool shouldRenderInterpolated, IByteWriter byteWriter)
    {
        this.source = source;
        Width = width;
        Height = height;
        DeclaredWithInterpolation = shouldRenderInterpolated;
        this.byteWriter = byteWriter;
    }

    public unsafe ValueTask RenderPbgraAsync(byte* buffer)
    {
        var x = new BitmapWriter(buffer, source, Width, Height, byteWriter);
        return InnerRenderAsync(x);
    }

    private async ValueTask InnerRenderAsync(BitmapWriter c)
    {
        int row = 0;
        int column = 0;
        while (c.LoadLPixels(await c.ReadAsync().CA(), ref row, ref column))
        {
            /* do nothing*/
        }
    }
}

internal unsafe readonly partial struct BitmapWriter
{
    [FromConstructor]private readonly byte* buffer;
    [FromConstructor]private readonly PipeReader reader;
    [FromConstructor]private readonly int width;
    [FromConstructor]private readonly int height;
    [FromConstructor]private readonly IByteWriter writer;

    public ValueTask<ReadResult> ReadAsync() => reader.ReadAsync();

    public bool LoadLPixels(ReadResult readResult, ref int row, ref int col)
    {
        if (readResult.IsCompleted && !EnoughBytesToRead(readResult.Buffer.Length))
            return false;
        var seq = new SequenceReader<byte>(readResult.Buffer);
        LoadBytes(ref row, ref col, ref seq);

        reader.AdvanceTo(seq.Position, readResult.Buffer.End);
        return row < height;
    }

    private void LoadBytes(ref int row, ref int col, ref SequenceReader<byte> seq)
    {
        while (EnoughBytesToRead(seq.Remaining))
        {
            byte* localPointer = buffer + PixelOffset(row, col);
            byte* oneOffEnd = localPointer + ((width - col) * 4);
            writer.WriteBytes(ref seq, ref localPointer, oneOffEnd);
            if (oneOffEnd == localPointer)
            {
                row++;
                if (row >= height) return;
                col = 0;
            }
            else
            {
                col = width - (int)(oneOffEnd - localPointer) / 4;
            }
        }
    }

    private bool EnoughBytesToRead(long bytesRemaining) => 
        bytesRemaining >= writer.MinimumInputSize;

    private int PixelOffset(int row, int col) => 4 * PixelPosition(row, col);
    private int PixelPosition(int row, int col) => (col + (row * width));
}