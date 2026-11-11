using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System;
using System.IO;
using System.Threading.Tasks;

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
            using var bufferFactory = 
            new UnmanagedMemoryManager<DeviceColor>((DeviceColor*)buffer, Width * Height);
        return InnerRenderAsync(bufferFactory.Memory);
    }

    private async ValueTask InnerRenderAsync(Memory<DeviceColor> target)
    {
        using var source = await attr.Stream.StreamContentAsync(context: this).CA();
        
        switch (source)
        {
            case IInternalBufferBytes ibb:
                WriteCompleteBuffer(ibb.AsMemory().Span, target);
                break;
            case MemoryStream ms when ms.TryGetBuffer (out var buffSeg) :
                WriteCompleteBuffer(buffSeg.AsMemory().Span, target);
                break;
            default:
                await StreamBitmapBitsAsync(source, target);
                break;
        }
    }

    void WriteCompleteBuffer(ReadOnlySpan<byte> input, Memory<DeviceColor> output)
    {
        var writer = byteWriter(finalColorSpace);
        var src = new SpanParsingBuffer(input);
        var dest = output.Span;
        writer.WriteBytes(ref src, ref dest);

    }
    async ValueTask StreamBitmapBitsAsync(Stream source, Memory<DeviceColor> target)
    {
        var writer = byteWriter(finalColorSpace);
        using var sourceBuffer = new ParsingBuffer(source.AsStreamReader());
        while (target.Length > 0 && (sourceBuffer.CurrentLength > 0 || !sourceBuffer.DoneReadingSource))
        {
            await sourceBuffer.ReadBytesAsync().CA();
            var spanReader = new SpanParsingBuffer(sourceBuffer.Peek());
            var targetSpan = target.Span;
            writer.WriteBytes(ref spanReader, ref targetSpan);
            sourceBuffer.Advance((int)spanReader.RelativePosition);
            target = target[^targetSpan.Length..];
        }
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