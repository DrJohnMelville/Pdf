using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class PatternDictionarySegment : DictionarySegment
{
    public PatternDictionarySegment(IBinaryBitmap[] exportedSymbols) :
        base(SegmentType.PatternDictionary, exportedSymbols.AsMemory())

    {
    }
}