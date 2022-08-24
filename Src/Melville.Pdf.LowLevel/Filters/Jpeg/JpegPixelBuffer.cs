using System;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

//This buffer represents a single row of MCUs or 8 rows of pixels with enough MCUs to cover the width.
public readonly struct JpegPixelBuffer
{
    private readonly byte[] data;
    private readonly int stride;
    private readonly int dataLineLength;
    private readonly int mcuWidth;
    
    public int WidthInBlocks { get; }

    public JpegPixelBuffer(int widthPixeks, int componentCount) : this()
    {
        WidthInBlocks = ComputeWidthInWholeBlocks(widthPixeks);
        mcuWidth = componentCount * 8;
        stride = WidthInBlocks*mcuWidth;
        data = new byte[8 * stride];
        dataLineLength = componentCount * widthPixeks;
    }
    private static int ComputeWidthInWholeBlocks(int width) => (width + 7) / 8;


    public Span<byte> McuLine(int mcu, int line) => 
        data.AsSpan((line * stride) + (mcu * mcuWidth), mcuWidth);

    public Span<byte> PartialByteLine(int row, int startingColumn) => 
        data.AsSpan((row * stride) + startingColumn, dataLineLength - startingColumn);

    public bool HasMoreBytesInLine(int startingPos) => startingPos < dataLineLength;
    
}