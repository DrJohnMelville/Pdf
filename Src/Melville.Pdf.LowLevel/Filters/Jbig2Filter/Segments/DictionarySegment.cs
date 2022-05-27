using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class DictionarySegment: Segment
{
    public IBinaryBitmap[] AllSymbols { get; }
    public Memory<IBinaryBitmap> ExportedSymbols { get; }

    protected DictionarySegment(
        SegmentType type, IBinaryBitmap[] allSymbols, Memory<IBinaryBitmap> exportedSymbols) : base(type)
    {
        AllSymbols = allSymbols;
        ExportedSymbols = exportedSymbols;
    }
}