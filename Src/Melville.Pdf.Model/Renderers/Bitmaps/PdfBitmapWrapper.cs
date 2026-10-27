using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Streams;

namespace Melville.Pdf.Model.Renderers.Bitmaps;

internal class PdfBitmapWrapper(
    BitmapRenderParameters attr,
    bool shouldRenderInterpolated,
    Func<IColorSpace,IByteWriter> byteWriter,
    IColorSpace colorSpace
    ) : IPdfBitmap, IReportColorComponents
{
    public int Width => attr.Width;
    public int Height => attr.Height;
    public bool DeclaredWithInterpolation => shouldRenderInterpolated;
    private IColorSpace finalColorSpace = colorSpace;

    public unsafe ValueTask RenderPbgraAsync(byte* buffer)
    {
        var x = new BitmapWriter(buffer, Width, Height);
        return InnerRenderAsync(x);
    }

    private async ValueTask InnerRenderAsync(BitmapWriter c)
    {
        using var source = await attr.Stream.StreamContentAsync(context: this).CA();
#warning this is inefficient  we do use a buffer if it is there, but would like to stream partial values.

        var row = 0;
        var column = 0;
        var writer = byteWriter(finalColorSpace);
        ISequenceSource seq = source as ISequenceSource ?? await FillBuffer(source);
        var src = new SequenceReader<byte>(seq.AsSequence());
        c.LoadLPixels(ref src, ref row, ref column, writer, out var readTo);
        if (seq is IDisposable disp && seq != source) disp.Dispose();
    }
    async Task<ISequenceSource> FillBuffer(Stream source)
    {
        var buffer = new MultiMemoryBuffer();
        using var memStr = buffer.CreateWriter();
        await source.AsStreamReader().CopyToAsync(memStr);
        return buffer;
    }

    /// <inheritdoc />
    public async ValueTask ReportColorComponentsAsync(int components)
    {
        //Sometimes I run into JPX images that have an incompatible colorspace, that is that the image
        // file reports more color componets that the declared colorspace can hande.  When this occurs
        // we use a default colorspace based on the mumber of color components.
        if (components == finalColorSpace.ExpectedComponents) return;
        finalColorSpace = components switch
        {
            1 => DeviceGray.Instance,
            3 => DeviceRgb.Instance,
            4 => await ColorSpaceFactory.CreateCmykColorSpaceAsync().CA(),
            _ => throw new PdfParseException(
                $"JPX image has {components} components, but color space only supports {finalColorSpace.ExpectedComponents}")
        };
    }
}

internal readonly unsafe partial struct BitmapWriter
{
    [FromConstructor]private readonly byte* buffer;
    [FromConstructor]private readonly int width;
    [FromConstructor]private readonly int height;
    public bool LoadLPixels(ref SequenceReader<byte> seq, ref int row, ref int col,
        IByteWriter writer, out SequencePosition sp)
    {
        sp = default;
        if (!EnoughBytesToRead(seq.Length, writer.MinimumInputSize))
            return false;
        LoadBytes(ref row, ref col, writer, ref seq);
        sp = seq.Position;
        return row < height;
    }

    private void LoadBytes(ref int row, ref int col, IByteWriter writer, ref SequenceReader<byte> seq)
    {
        while (EnoughBytesToRead(seq.Remaining, writer.MinimumInputSize))
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

    private bool EnoughBytesToRead(long bytesRemaining, int minimumInput) => 
        bytesRemaining >= minimumInput;

    private int PixelOffset(int row, int col) => 4 * PixelPosition(row, col);
    private int PixelPosition(int row, int col) => (col + (row * width));
}