using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public readonly struct JpegPixelBuffer
{
    private readonly byte[] data;
    private readonly int stride;
    private readonly int componentCount;
    
    public JpegPixelBuffer(int width, int componentCount) : this()
    {
        this.componentCount = componentCount;
        this.stride = width*this.componentCount;
        data = new byte[8 * stride];
    }

}

public partial class ReadJpegStream: DefaultBaseStream, IImageSizeStream
{
    private readonly PipeReader source;
    private readonly JpegHeaderData context;
    
    private JpegPixelBuffer pixels;
    private int totalLinesRead = 0;
    private readonly int widthInBlocks;

    public ReadJpegStream(PipeReader source, JpegHeaderData context) : 
        base(true, false, false)
    {
        this.source = source;
        this.context = context;
        widthInBlocks = ComputeWidthInWholeBlocks(context.Width);
        pixels = new JpegPixelBuffer(widthInBlocks, context.ImageComponents);
    }
    
    private static int ComputeWidthInWholeBlocks(int width)
    {
        return (width + 7) / 8;
    }


    [DelegateTo] private IImageSizeStream SizeInfo => context;
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        await LoadData().CA();
        return 0;
    }

    private async ValueTask LoadData()
    {
        
    }
}