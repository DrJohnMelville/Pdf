using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class PatternDictionarySegment : DictionarySegment
{
    public PatternDictionarySegment(IBinaryBitmap[] exportedSymbols) :
        base(SegmentType.PatternDictionary, exportedSymbols.AsMemory())

    {
    }
}