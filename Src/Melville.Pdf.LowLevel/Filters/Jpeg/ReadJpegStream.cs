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
    private readonly int[] undoZArray;

    public ReadJpegStream(AsyncBitSource source, JpegHeaderData context) : 
        base(true, false, false)
    {
        this.source = source;
        this.context = context;
        widthInBlocks = ComputeWidthInWholeBlocks(context.Width);
        pixels = new JpegPixelBuffer(widthInBlocks, context.ImageComponents);
        undoZArray = pixels.ZizZagDecodingOffsets();
        Debug.Assert(undoZArray.Length == 63);
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
                var componentBase = offset.Slice(j);
                SetDcValue(componentBase, (byte)await context.ReadDcAsync(j, source).CA());
                for (int acBytes = 0; acBytes < 63; acBytes++)
                {
                    var (leadingZeros, number) = await context.ReadAcAsync(j, source).CA();
                    //NEED TO HANDLE (0,0) as the end of bytes signmal
                    for (int k = 0; k < leadingZeros; k++)
                    {
                        SetByte(componentBase, acBytes++, 0);
                    }
                    
                    if (acBytes < 63)
                    {
                        SetByte(componentBase, acBytes, (byte)number);
                    }
                }
            }
        }
    }

    private void SetDcValue(Memory<byte> componentBase, byte value) => componentBase.Span[0] = value;

    private void SetByte(in Memory<byte> componentBase, int acBytes, byte value) =>
        componentBase.Span[undoZArray[acBytes]] = value;
}