using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Predictors
{
    public static class ScanLineLengthComputer
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
    public abstract class TiffPredictor2Filter: IStreamFilterDefinition
    {
        private int bitsPerColor;
        private int groupsPerRow;
        private int[] buffer;
        private int minBytes;
        private BitReader reader;
        private BitWriter writer;
        private int currentCol = 0;

        protected TiffPredictor2Filter(int colors, int bitsPerColor, int colorsPerRow)
        {
            this.bitsPerColor = bitsPerColor;
            groupsPerRow = ScanLineLengthComputer.ComputeGroupsPerRow(colors, bitsPerColor, colorsPerRow, bitsPerColor);
            minBytes = 2 + ((bitsPerColor + 7) / 8);
            buffer = new int[colors];
            reader = new BitReader();
            writer = new BitWriter();
        }

        public int MinWriteSize => minBytes+1;

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done) Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            var destPosition = 0;
            while (destPosition + minBytes < destination.Length && 
                   reader.TryRead(bitsPerColor, ref source, out var readValue))
            {
                destPosition += writer.WriteBits(
                    ComputeFilterResult(readValue), bitsPerColor, destination[destPosition..]);
                currentCol++;
                if (currentCol >= groupsPerRow)
                {
                    currentCol = 0;
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            return (source.Position, destPosition, false);
        }
        
        public (SequencePosition SourceConsumed, int bytesWritten, bool Done) FinalConvert(ref SequenceReader<byte> source,
            ref Span<byte> destination)
        {
            if (destination.Length < minBytes) return (source.Position, 0, false);
            Debug.Assert(!source.TryRead(out _));
            return (source.Position, writer.FinishWrite(destination), true);
        }

        private int ComputeFilterResult(int readValue) => ComputeAdjustedValue(readValue, ref buffer[SelectBufferSlot()]);

        private int SelectBufferSlot() => currentCol % buffer.Length;

        protected abstract int ComputeAdjustedValue(int readValue, ref int bufferSlot);
    }
    public class TiffPredictor2Encoder: TiffPredictor2Filter
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

    public class TiffPredictor2Decoder : TiffPredictor2Filter
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
}