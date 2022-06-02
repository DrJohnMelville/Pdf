using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct TextRegionSegmentParser
{
    public static TextRegionSegment Parse(SequenceReader<byte> reader, ReadOnlySpan<Segment> referencedSegments)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var regionFlags = new TextRegionFlags(reader.ReadBigEndianUint16());

        var parser = new TextRegionSegmentParser(reader, regionHead, regionFlags, referencedSegments);
        return new TextRegionSegment(SegmentType.IntermediateTextRegion, regionHead, parser.CreateBitmap());
    }

    private SequenceReader<byte> reader;
    private readonly RegionHeader regionHead;
    private readonly TextRegionFlags regionFlags;
    private readonly ReadOnlySpan<Segment> referencedSegments;

    private TextRegionSegmentParser(
        SequenceReader<byte> reader, RegionHeader regionHead, TextRegionFlags regionFlags,
        ReadOnlySpan<Segment> referencedSegments)
    {
        this.reader = reader;
        this.regionHead = regionHead;
        this.regionFlags = regionFlags;
        this.referencedSegments = referencedSegments;
    }

    private BinaryBitmap CreateBitmap()
    {
        if (!regionFlags.UseHuffman)
            throw new NotImplementedException("Only supports huffman encoding");
        var huffmanFlags = new TextRegionHuffmanFlags(
            regionFlags.UseHuffman ? reader.ReadBigEndianUint16():(ushort)0);
        if (regionFlags.UsesRefinement)
            throw new NotImplementedException("Refinement is not supported yet.");
        var charactersToRead = (int)reader.ReadBigEndianUint32();

        var charDecoder = ParseCharacterHuffmanTable();
        
        var binaryBitmap = CreateTargetBitmap();

        var symbolParser = CreateSymbolWriter(huffmanFlags, charDecoder, charactersToRead, 
            CreateBitmapWriter(binaryBitmap));

        symbolParser.Decode(ref reader);
        
        return binaryBitmap;
    }

    private SymbolWriter CreateSymbolWriter(TextRegionHuffmanFlags huffmanFlags,
        HuffmanLine[] huffmanCharacterDecoder, int charactersToRead, in BinaryBitmapWriter writer)
    {
        var remainingTableSpans = referencedSegments;
        
        if (regionFlags.UseHuffman)
        return new SymbolWriter(writer, regionFlags,
            new HuffmanIntegerDecoder()
            {
                SymbolIdContext = huffmanCharacterDecoder,
                FirstSContext = huffmanFlags.SbhuffFs.GetTableLines(ref remainingTableSpans),
                DeltaSContext = huffmanFlags.SbhuffDs.GetTableLines(ref remainingTableSpans),
                DeltaTContext = huffmanFlags.SbhuffDt.GetTableLines(ref remainingTableSpans),
                TCoordinateContext = DirectBitstreamReaders.FromLogStripSize(regionFlags.LogStripSize),
                RefinementDeltaWidthContext = huffmanFlags.SbhuffRdw.GetTableLines(ref remainingTableSpans),
                RefinementDeltaHeightContext =  huffmanFlags.SbhuffRdh.GetTableLines(ref remainingTableSpans),
                RefinementXContext =  huffmanFlags.SbhuffRdx.GetTableLines(ref remainingTableSpans),
                RefinementYContext =  huffmanFlags.SbhuffRdy.GetTableLines(ref remainingTableSpans),
                RefinementSizeContext =  huffmanFlags.SbHuffRSize.GetTableLines(ref remainingTableSpans),
            }, new CharacterDictionary(referencedSegments), charactersToRead);

        throw new NotImplementedException("Must use huffman encoding");
    }

    private BinaryBitmapWriter CreateBitmapWriter(BinaryBitmap binaryBitmap) =>
        new(binaryBitmap, regionFlags.Transposed, regionFlags.ReferenceCorner, 
            regionFlags.CombinationOperator);

    private BinaryBitmap CreateTargetBitmap()
    {
        var binaryBitmap = regionHead.CreateTargetBitmap();
        SetBitmapBackground(binaryBitmap);
        return binaryBitmap;
    }

    private void SetBitmapBackground(BinaryBitmap binaryBitmap)
    {
        if (regionFlags.DefaultPixel) binaryBitmap.FillBlack();
    }

    private HuffmanLine[] ParseCharacterHuffmanTable()
    {
        Debug.Assert(regionFlags.UseHuffman);
        var sourceChars = CountSourceBitmaps();
        var lines = new HuffmanLine[sourceChars];
        TextSegmentSymbolTableParser.Parse(ref reader, lines);
        return lines;
    }

    private int CountSourceBitmaps()
    {
        var ret = 0;
        foreach (var segment in referencedSegments)
        {
            if (segment is DictionarySegment sds) ret += sds.ExportedSymbols.Length;
        }

        return ret;
    }
}