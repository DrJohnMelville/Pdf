using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

public class PatternDictionarySegment : DictionarySegment
{
    public PatternDictionarySegment(IBinaryBitmap[] exportedSymbols) :
        base(SegmentType.PatternDictionary, exportedSymbols.AsMemory())

    {
    }
}