using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;
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
        var huffmanFlags = new TextRegionHuffmanFlags(
            regionFlags.UseHuffman ? reader.ReadBigEndianUint16():(ushort)0);

        var refinementTemplate = regionFlags.UsesRefinement
            ? new RefinementTemplateSet(ref reader, regionFlags.RefinementTemplate)
            : new RefinementTemplateSet();
        
        var charactersToRead = (int)reader.ReadBigEndianUint32();

        IEncodedReader encodedReader = regionFlags.UseHuffman?
            ParseHuffmanDecoder(huffmanFlags):
            CreateArithmeticDecoder();
        var binaryBitmap = CreateTargetBitmap();

        var symbolParser = new SymbolWriter(CreateBitmapWriter(binaryBitmap),
            encodedReader, referencedSegments, ReadOnlySpan<IBinaryBitmap>.Empty, charactersToRead,
            regionFlags.StripSize, regionFlags.DefaultCharacteSpacing, 
            regionFlags.UsesRefinement, refinementTemplate);
                // To support refinement we have to actually parse the refinementTemplateSet
                // see the feature guard clause above
        symbolParser.Decode(ref reader);
        
        return binaryBitmap;
    }

    private IEncodedReader ParseHuffmanDecoder(TextRegionHuffmanFlags huffmanFlags)
    {
        var remainingTableSpans = referencedSegments;
        var intDecoder = new HuffmanIntegerDecoder()
        {
            SymbolIdContext = ParseCharacterHuffmanTable(),
            FirstSContext = huffmanFlags.SbhuffFs.GetTableLines(ref remainingTableSpans),
            DeltaSContext = huffmanFlags.SbhuffDs.GetTableLines(ref remainingTableSpans),
            DeltaTContext = huffmanFlags.SbhuffDt.GetTableLines(ref remainingTableSpans),
            TCoordinateContext = DirectBitstreamReaders.FromBitLength(regionFlags.LogStripSize),
            RefinementDeltaWidthContext = huffmanFlags.SbhuffRdw.GetTableLines(ref remainingTableSpans),
            RefinementDeltaHeightContext = huffmanFlags.SbhuffRdh.GetTableLines(ref remainingTableSpans),
            RefinementXContext = huffmanFlags.SbhuffRdx.GetTableLines(ref remainingTableSpans),
            RefinementYContext = huffmanFlags.SbhuffRdy.GetTableLines(ref remainingTableSpans),
            RefinementSizeContext = huffmanFlags.SbHuffRSize.GetTableLines(ref remainingTableSpans),
            RIBitContext = DirectBitstreamReaders.OneBit
        };
        return intDecoder;
    }

    private IEncodedReader CreateArithmeticDecoder() =>
        new ArithmeticIntegerDecoder(new ArithmeticBitmapReaderContext())
        {
            SymbolIdContext = ArithmiticSymbolIdContext(),
            FirstSContext = new ContextStateDict(9),
            DeltaSContext = new ContextStateDict(9),
            DeltaTContext = new ContextStateDict(9),
            TCoordinateContext = new ContextStateDict(9),
            RefinementDeltaWidthContext = new ContextStateDict(9),
            RefinementDeltaHeightContext = new ContextStateDict(9),
            RefinementXContext = new ContextStateDict(9),
            RefinementYContext = new ContextStateDict(9),
            RefinementSizeContext = new ContextStateDict(9),
            RIBitContext = new ContextStateDict(9)
        };

    private ContextStateDict ArithmiticSymbolIdContext()
    {
        var maxSymbolSize = IntLog.CeilingLog2Of((uint)referencedSegments.CountSourceBitmaps());
        return new ContextStateDict(maxSymbolSize, maxSymbolSize);
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
        var sourceChars = referencedSegments.CountSourceBitmaps();
        var lines = new HuffmanLine[sourceChars];
        TextSegmentSymbolTableParser.Parse(ref reader, lines);
        return lines;
    }
}