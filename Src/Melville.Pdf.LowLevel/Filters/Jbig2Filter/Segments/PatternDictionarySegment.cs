using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.FileOrganization;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class PatternDictionarySegment : DictionarySegment
{
    public PatternDictionarySegment(IBinaryBitmap[] exportedSymbols) :
        base(SegmentType.PatternDictionary, exportedSymbols.AsMemory())

    {
    }
}