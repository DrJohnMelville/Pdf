using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;

public readonly ref struct GenericRegionReader
{
    private readonly BinaryBitmap target;
    private readonly bool useMMr;

    public GenericRegionReader(BinaryBitmap target, bool useMMr)
    {
        this.target = target;
        this.useMMr = useMMr;
    }

    public void ReadFrom(ref SequenceReader<byte> reader, bool requireTerminator)
    {
        if (!useMMr)
            throw new NotImplementedException("Uses only mmr encoding for now");
        target.ReadMmrEncodedBitmap(ref reader, requireTerminator);
        
    }
}