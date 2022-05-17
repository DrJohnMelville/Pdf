using System;
using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public static class TextRegionSegmentParser
{
    public static TextRegionSegment Parse(ref SequenceReader<byte> reader, uint segmentNumber)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var regionFlags = new TextRegionFlags(reader.ReadBigEndianUint16());
        var huffmanFlags = new TextRegionHuffmanFlags(
            regionFlags.UseHuffman ? reader.ReadBigEndianUint16():(ushort)0);
        if (regionFlags.UsesRefinement)
            throw new NotImplementedException("Refinement is not supported yet.");
        var instances = reader.ReadBigEndianUint32();

        Span<int> runCodes = stackalloc int[35];
        for (int i = 0; i < 17; i++)
        {
            reader.TryRead(out var datum);
            runCodes[2 * i] = datum >> 4;
            runCodes[2 * i + 1] = datum & 7;
        }

        var src = new BitSource(reader);
        runCodes[34] = src.ReadInt(4);
        
        

        return new TextRegionSegment(SegmentType.IntermediateTextRegion, segmentNumber,
            new BinaryBitmap((int)regionHead.Height, (int)regionHead.Width));
    }
    
    
}

