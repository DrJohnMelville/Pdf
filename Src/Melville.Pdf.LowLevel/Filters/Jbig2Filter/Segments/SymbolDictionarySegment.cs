using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class SymbolDictionarySegment : Segment
{
    public IBinaryBitmap[] AllSymbols { get; }
    public Memory<IBinaryBitmap> ExportedSymbols { get; }
    
    public SymbolDictionarySegment(IBinaryBitmap[] allSymbols, Memory<IBinaryBitmap> exportedSymbols) : 
        base(SegmentType.SymbolDictionary)
    {
        AllSymbols = allSymbols;
        ExportedSymbols = exportedSymbols;
    }
}