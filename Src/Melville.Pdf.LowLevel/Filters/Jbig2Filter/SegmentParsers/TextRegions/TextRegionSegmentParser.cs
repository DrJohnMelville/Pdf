using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using SequenceReaderExtensions = System.Buffers.SequenceReaderExtensions;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public ref struct TextRegionSegmentParser
{
    public static TextRegionSegment Parse(SequenceReader<byte> reader, ReadOnlySpan<Segment> referencedSegments)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var regionFlags = new TextRegionFlags(reader.ReadBigEndianUint16());

        var parser = new TextRegionSegmentParser(reader, regionHead, regionFlags, referencedSegments);
        return new TextRegionSegment(SegmentType.IntermediateTextRegion, parser.CreateBitmap());
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

        symbolParser.Decode();
        
        return binaryBitmap;
    }

    private SymbolWriter CreateSymbolWriter(TextRegionHuffmanFlags huffmanFlags,
        CharacterDecoder charDecoder, int charactersToRead, in BinaryBitmapWriter writer)
    {
        var remainingTableSpans = referencedSegments;
        return new SymbolWriter(writer, 
            new BitSource(reader), regionFlags,
            huffmanFlags.SbhuffFs.GetTable(ref remainingTableSpans),
            huffmanFlags.SbhuffDs.GetTable(ref remainingTableSpans),
            huffmanFlags.SbhuffDt.GetTable(ref remainingTableSpans),
            CreateDeltaTDecoder(),
            huffmanFlags.SbhuffRdw.GetTable(ref remainingTableSpans),
            huffmanFlags.SbhuffRdh.GetTable(ref remainingTableSpans),
            huffmanFlags.SbhuffRdx.GetTable(ref remainingTableSpans),
            huffmanFlags.SbhuffRdy.GetTable(ref remainingTableSpans),
            huffmanFlags.SbHuffRSize.GetTable(ref remainingTableSpans),
            charDecoder, charactersToRead);
    }

    private BinaryBitmapWriter CreateBitmapWriter(BinaryBitmap binaryBitmap) =>
        new(binaryBitmap, regionFlags.Transposed, regionFlags.ReferenceCorner, 
            regionFlags.CombinationOperator);

    private IIntegerDecoder CreateDeltaTDecoder()
    {
        if (regionFlags.LogStripSize == 0) return FakeReaderReturnsZero.Instance;
        if (regionFlags.UseHuffman) return DirectReader.Instances[regionFlags.LogStripSize];
        throw new NotImplementedException("Must use huffman coding. see ITU t88 sec 6.4.8");
    }

    private BinaryBitmap CreateTargetBitmap()
    {
        var binaryBitmap = new BinaryBitmap((int)regionHead.Height, (int)regionHead.Width);
        SetBitmapBackground(binaryBitmap);
        return binaryBitmap;
    }

    private void SetBitmapBackground(BinaryBitmap binaryBitmap)
    {
        if (regionFlags.DefaultPixel) binaryBitmap.FillBlack();
    }

    private CharacterDecoder ParseCharacterHuffmanTable()
    {
        Debug.Assert(regionFlags.UseHuffman);
        var sourceChars = CountSourceBitmaps();
        var lines = new HuffmanLine[sourceChars];
        TextSegmentSymbolTableParser.Parse(ref reader, lines);
        var charDecoder = new CharacterDecoder(new HuffmanTable(lines), referencedSegments);
        return charDecoder;
    }

    private int CountSourceBitmaps()
    {
        var ret = 0;
        foreach (var segment in referencedSegments)
        {
            if (segment is SymbolDictionarySegment sds) ret += sds.ExportedSymbols.Length;
        }

        return ret;
    }
}