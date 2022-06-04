using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public sealed class RefinementBitmapReader : IIndividualBitmapReader
{
    public static RefinementBitmapReader Instance = new RefinementBitmapReader();
    private RefinementBitmapReader() { }

    public void ReadBitmap(ref SequenceReader<byte> source, SymbolParser reader, BinaryBitmap bitmap)
    {
        var numSyms = reader.EncodedReader.AggregationSymbolInstances(ref source);
        if (numSyms != 1) throw new NotImplementedException("Multi character bitmap");
        ReadSingleSourceRefinementBitmap(ref source, reader, bitmap);
    }

    private static void ReadSingleSourceRefinementBitmap(ref SequenceReader<byte> source, SymbolParser reader,
        BinaryBitmap bitmap)
    {
        var symbolId = reader.EncodedReader.SymbolId(ref source);
        var refineX = reader.EncodedReader.RefinementX(ref source);
        var refineY = reader.EncodedReader.RefinementY(ref source);
        reader.EncodedReader.InvokeSymbolRefinement(bitmap,
            reader.ReferencedSymbol(symbolId), refineX, refineY, false, reader.RefinementTemplateSet,
            ref source);
    }
}