using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class PatternDictionarySegment : Segment
{
    public IBinaryBitmap[] Patterns { get; }

    public PatternDictionarySegment(IBinaryBitmap[] patterns) : base(SegmentType.PatternDictionary)
    {
        Patterns = patterns;
    }
}