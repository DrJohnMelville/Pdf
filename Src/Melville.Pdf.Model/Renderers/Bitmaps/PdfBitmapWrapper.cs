using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class PdfBitmapWrapper(
    BitmapRenderParameters attr,
    bool shouldRenderInterpolated,
    IByteWriter byteWriter)
    : IPdfBitmap
{
    public int Width => attr.Width;
    public int Height => attr.Height;
    public bool DeclaredWithInterpolation => shouldRenderInterpolated;

    public unsafe ValueTask RenderPbgraAsync(byte* buffer)
    {
        var x = new BitmapWriter(buffer, Width, Height, byteWriter);
        return InnerRenderAsync(x);
    }

    private async ValueTask InnerRenderAsync(BitmapWriter c)
    {
        using var source = MultiplexSourceFactory.SingleReaderForStream(
            await attr.Stream.StreamContentAsync().CA());
        int row = 0;
        int column = 0;
        while (true)
        {
            var seq = await source.ReadAsync().CA();
            if (!c.LoadLPixels(seq, ref row, ref column, out var readTo)) return;
            source.AdvanceTo(readTo, seq.Buffer.End);
        }
    }
}

internal unsafe readonly partial struct BitmapWriter
{
    [FromConstructor]private readonly byte* buffer;
    [FromConstructor]private readonly int width;
    [FromConstructor]private readonly int height;
    [FromConstructor]private readonly IByteWriter writer;


    public bool LoadLPixels(ReadResult readResult, ref int row, ref int col, out SequencePosition sp)
    {
        sp = default;
        if (readResult.IsCompleted && !EnoughBytesToRead(readResult.Buffer.Length))
            return false;
        var seq = new SequenceReader<byte>(readResult.Buffer);
        LoadBytes(ref row, ref col, ref seq);
        sp = seq.Position;
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