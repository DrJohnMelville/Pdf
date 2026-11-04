using Melville.Parsing.StreamFilters;
using Melville.Parsing.VariableBitEncoding;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.Predictors;

internal static class ScanLineLengthComputer
{
    public static int ComputeGroupsPerRow(int colors, int bitsPerColor, int colorsPerRow, int groupSize)
    {
        // groups of bitspercolor are packed into bytes within a line, but then at the end of a line
        // and residual bits in the last byte are wasted.  The legal groupsizes (1,2,4,8,and 16) all can be padded
        // into a byte. So the minimum number of bytes to hold a line is guarentted to be an integer number of
        // groups.
        var bitsPerRow = colors * bitsPerColor * colorsPerRow;
        var bytesPerRow = BitsToBytesRoundUp(bitsPerRow);
        return (bytesPerRow * 8) / groupSize;
    }

    public static int BitsToBytesRoundUp(int bitsPerRow) => (bitsPerRow + 7) / 8;
}

internal abstract class TiffPredictor2Filter: IStreamFilterDefinition
{
    private readonly int bitsPerColor;
    private readonly int groupsPerRow;
    private readonly int[] buffer;
    private readonly int minBytes;
    private readonly BitReader reader = new();
    private int currentCol = 0;

    protected TiffPredictor2Filter(int colors, int bitsPerColor, int colorsPerRow)
    {
        this.bitsPerColor = bitsPerColor;
        groupsPerRow = ScanLineLengthComputer.ComputeGroupsPerRow(colors, bitsPerColor, colorsPerRow, bitsPerColor);
        minBytes = 2 + ((bitsPerColor + 7) / 8);
        buffer = new int[colors];
    }

    public int MinWriteSize => minBytes+1;

    private readonly BitWritingBuffer writer = new();

    public bool Convert(ref SpanParsingBuffer input, ref Span<byte> output)
    {
        while (writer.TryWriteToMovingBuffer(ref output) &&
               reader.TryRead(bitsPerColor, ref input, out var readValue))
        {
            writer.WriteBits(ComputeFilterResult(readValue), bitsPerColor);
            currentCol++;
            if (currentCol >= groupsPerRow)
            {
                currentCol = 0;
                buffer.AsSpan().Clear();
            }
        }
        return false;
    }

    public bool FinalConvert(ref SpanParsingBuffer input, ref Span<byte> output) => Convert(ref input, ref output);

    private int ComputeFilterResult(int readValue) => ComputeAdjustedValue(readValue, ref buffer[SelectBufferSlot()]);

    private int SelectBufferSlot() => currentCol % buffer.Length;

    protected abstract int ComputeAdjustedValue(int readValue, ref int bufferSlot);
}

internal class TiffPredictor2Encoder: TiffPredictor2Filter
{
    public TiffPredictor2Encoder(int colors, int bitsPerColor, int colorsPerRow) : 
        base(colors, bitsPerColor, colorsPerRow)
    {
    }
    protected override int ComputeAdjustedValue(int readValue, ref int bufferSlot)
    {
        var ret = readValue - bufferSlot;
        bufferSlot = readValue;
        return ret;
    }
}

internal class TiffPredictor2Decoder : TiffPredictor2Filter
{
    public TiffPredictor2Decoder(int colors, int bitsPerColor, int colorsPerRow) : base(colors, bitsPerColor, colorsPerRow)
    {
    }

    protected override int ComputeAdjustedValue(int readValue, ref int bufferSlot)
    {
        var ret = readValue + bufferSlot;
        bufferSlot = ret;
        return ret;
    }
}