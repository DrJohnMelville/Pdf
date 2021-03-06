using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public sealed class RefinementBitmapReader : IIndividualBitmapReader
{
    public static RefinementBitmapReader Instance = new();
    private RefinementBitmapReader() { }

    public void ReadBitmap(ref SequenceReader<byte> source, ref SymbolParser reader, BinaryBitmap bitmap)
    {   //  this method begins section 6.5.8.2 in the specification
        var numSyms = reader.EncodedReader.AggregationSymbolInstances(ref source);
        if (numSyms == 1)
            ReadSingleSourceRefinementBitmap(ref source, ref reader, bitmap);
        else
            ReadMultiSourceRefinedBitmap(ref source, ref reader, bitmap, numSyms);
    }

    private static void ReadMultiSourceRefinedBitmap(
        ref SequenceReader<byte> source, ref SymbolParser reader, BinaryBitmap bitmap, int numSyms) =>
        new SymbolWriter(CreateBitmapWriter(bitmap), reader.EncodedReader,
            reader.ReferencedSegments, reader.SymbolsBeingDecoded.Span, numSyms,
            1, 0, true, reader.RefinementTemplateSet).Decode(ref source);

    private static BinaryBitmapWriter CreateBitmapWriter(BinaryBitmap bitmap) => 
        new(bitmap, false, ReferenceCorner.TopLeft, CombinationOperator.Or);

    private static void ReadSingleSourceRefinementBitmap(ref SequenceReader<byte> source, ref SymbolParser reader,
        BinaryBitmap bitmap)
    {
        var referencedSymbol = ReadReferencedSymbol(ref source, ref reader);

        reader.EncodedReader.InvokeSymbolRefinement(bitmap, referencedSymbol, 0, reader.RefinementTemplateSet,
            ref source);
    }

    private static IBinaryBitmap ReadReferencedSymbol(ref SequenceReader<byte> source, ref SymbolParser reader)
    {
        var symbolId = reader.EncodedReader.SymbolId(ref source);
        var refineX = reader.EncodedReader.RefinementX(ref source);
        var refineY = reader.EncodedReader.RefinementY(ref source);
        var referencedSymbol = reader.ReferencedSymbol(symbolId);
        return OffsetBitmapFactory.Create(referencedSymbol, -refineY, -refineX);
    }
}