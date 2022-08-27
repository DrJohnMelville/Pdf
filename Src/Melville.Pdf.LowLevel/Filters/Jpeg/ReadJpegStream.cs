using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
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
    private int currentBlockLinesRead = 8;
    private int totalLinesToRead;
    private int bytesInLineRead = 0;

    public ReadJpegStream(
        AsyncBitSource source, int width, int height, int bitsPerComponent, 
        ComponentReader[] components): base(true,false,false)
    {
        this.source = source;
        Width = width;
        Height = height;
        BitsPerComponent = bitsPerComponent;
        Components = components;
        totalLinesToRead = height;
        pixels = new JpegPixelBuffer(width, ImageComponents);
    }
    
    
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
//        if (totalLinesToRead < 250) return 0;
        if (currentBlockLinesRead >= 8)
        {
            await LoadData().CA();
            currentBlockLinesRead = 0;
            bytesInLineRead = 0;
        }
        return TryCopyBuffer(buffer.Span);
    }

    private int TryCopyBuffer(Span<byte> bufferSpan)
    {
        var localReadSize = 0;
        while (currentBlockLinesRead < 8 && totalLinesToRead > 0)
        {
            if (pixels.HasMoreBytesInLine(bytesInLineRead))
            {
                var sourceLine = pixels.PartialByteLine(currentBlockLinesRead, bytesInLineRead);
                var target = bufferSpan[localReadSize..];
                if (target.Length < sourceLine.Length)
                {
                    sourceLine.Slice(0, target.Length).CopyTo(target);
                    localReadSize += target.Length;
                    return localReadSize;
                }
                sourceLine.CopyTo(target);
                localReadSize += sourceLine.Length;
            }
            currentBlockLinesRead++;
            bytesInLineRead = 0;
            totalLinesToRead--;
        }

        return localReadSize;
    }

    private async ValueTask LoadData()
    {
        for (int i = 0; i < pixels.WidthInBlocks; i++)
        {
            for (int j = 0; j < ImageComponents; j++)
            {
                await Components[j].ReadMcuAsync(source).CA();
            }
            TransferMCUToBuffer(i);
        }
    }

    private void TransferMCUToBuffer(int mcu)
    {
        int componentLength = Components.Length;
        for (int row = 0; row < 8; row++)
        {
            #warning -- height or width may not be a multiple of 8
            var offset = pixels.McuLine(mcu, row);
            var index = 0;
            for (int col = 0; col < 8; col++)
            {
                var innerOffset = offset.Slice(index, componentLength);
                ReadAllComponents(innerOffset, row, col);
                index += componentLength;
            }
        }
    }

    private void ReadAllComponents(in Span<byte> offset, int row, int col)
    {
        Debug.Assert(offset.Length == 3);
        double Y = Components[0].ReadValue(row, col);
        double Cr = Components[1].ReadValue(row, col) - 128;
        double Cb= Components[2].ReadValue(row, col) - 128;
  
        var R = Cr*(2-2*.299) + Y;
        var B = Cb * (2 - 2 * .114) + Y;
        var G = (Y - .114 * B - .299 * R) / .587;

        offset[0] = (byte)R.Clamp(0, 255);
        offset[1] = (byte)G.Clamp(0, 255);
        offset[2] = (byte)B.Clamp(0, 255);
    }
}