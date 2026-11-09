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
        var copier = new BitmapWriter(buffer, Width, Height);
        return InnerRenderAsync(copier);
    }

    private async ValueTask InnerRenderAsync(BitmapWriter copier)
    {
        using var source = await attr.Stream.StreamContentAsync(context: this).CA();
#warning this is inefficient  we do use a buffer if it is there, but would like to stream partial values.

        var writer = byteWriter(finalColorSpace);

#warning special case MemoryStream here because it has a buffer.
        var seq = source switch
        {
            IInternalBufferBytes ibb => ibb.AsMemory(),
            MemoryStream ms when ms.TryGetBuffer(out var buffSeg) => buffSeg.AsMemory(),
            _ => await FillBufferAsync(source)
        };
        var src = new SequenceReader<byte>(new ReadOnlySequence<byte>(seq));

        unsafe
        {
            var destPos = copier.Buffer;
            copier.LoadPixels(ref src, ref destPos, writer, out var readTo);

        }
    }

    async Task<ReadOnlyMemory<byte>> FillBufferAsync(Stream source)
    {
#warning terribly inefficent -- eventually we are going to stream these values.
        var buffer = new MemoryStream();
        await source.CopyToAsync(buffer);
        return buffer.ToArray();
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

internal readonly unsafe partial struct BitmapWriter(byte* buffer, int width, int height)
{
    public byte* Buffer => buffer;
    readonly byte* oneAfterEnd = -3 + buffer + (4 * width * height); // make sure we have enough room to write 4 bytes

    public bool LoadPixels(ref SequenceReader<byte> seq, ref byte* destPos,
        IByteWriter writer, out SequencePosition sp)
    {
        sp = default;
        if (!EnoughBytesToRead(seq.Length))
            return false;
        writer.WriteBytes(ref seq, ref destPos, oneAfterEnd);
        sp = seq.Position;
        return destPos >= oneAfterEnd;
    }

    private bool EnoughBytesToRead(long bytesRemaining) => 
        bytesRemaining >= 0;
}