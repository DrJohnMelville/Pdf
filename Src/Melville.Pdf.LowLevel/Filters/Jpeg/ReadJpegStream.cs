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
    
    public int Width { get; }
    public int Height { get; }
    public int BitsPerComponent { get; }
    public ComponentReader[] Components { get; }
    public int ImageComponents => Components.Length;
    
    private JpegPixelBuffer pixels;
    private int totalLinesRead = 0;
    private readonly int widthInBlocks;

    public ReadJpegStream(
        AsyncBitSource source, int width, int height, int bitsPerComponent, 
        ComponentReader[] components): base(true,false,false)
    {
        this.source = source;
        Width = width;
        Height = height;
        BitsPerComponent = bitsPerComponent;
        Components = components;
        widthInBlocks = ComputeWidthInWholeBlocks(Width);
        pixels = new JpegPixelBuffer(widthInBlocks, ImageComponents);
    }
    
    private static int ComputeWidthInWholeBlocks(int width)
    {
        return (width + 7) / 8;
    }
    
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
            for (int j = 0; j < ImageComponents; j++)
            {
                await Components[j].ReadMcuAsync(source).CA();
            }
        }
    }
}