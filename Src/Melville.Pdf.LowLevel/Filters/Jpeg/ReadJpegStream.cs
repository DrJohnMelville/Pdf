using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

// I relied on two main examples in writing this code
// https://yasoob.me/posts/understanding-and-writing-jpeg-decoder-in-python
//https://koushtav.me/jpeg/tutorial/c++/decoder/2019/03/02/lets-write-a-simple-jpeg-library-part-2
public partial class ReadJpegStream: DefaultBaseStream, IImageSizeStream
{
    private readonly AsyncBitSource source;
    private readonly JpegHeaderData context;
    
    private JpegPixelBuffer pixels;
    private int totalLinesRead = 0;
    private readonly int widthInBlocks;

    public ReadJpegStream(AsyncBitSource source, JpegHeaderData context) : 
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
        for (int i = 0; i < widthInBlocks; i++)
        {
            var offset = pixels.McuStart(i);
            for (int j = 0; j < context.ImageComponents; j++)
            {
                await context.Components[j].ReadMcuAsync(source).CA();
            }
        }
    }
}