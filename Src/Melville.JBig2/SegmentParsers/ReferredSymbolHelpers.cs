using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers;

public static class ReferredSymbolHelpers
{
    public static int CountSourceBitmaps(this ReadOnlySpan<Segment> segments)
    {
        var ret = 0;
        foreach (var segment in segments)
        {
            if (segment is DictionarySegment sds) ret += sds.ExportedSymbols.Length;
        }
 
        return ret;
    }

    public static IBinaryBitmap GetBitmap(this ReadOnlySpan<Segment> dictionaries, int index,
        ReadOnlySpan<IBinaryBitmap> resultSpan)
    {
        foreach (var segment in dictionaries)
        {
            if (segment is not DictionarySegment sds) continue;
            var exportedLength = sds.ExportedSymbols.Length;
            if (index < exportedLength) return sds.ExportedSymbols.Span[index];
            index -= exportedLength;
        }

        if (index < resultSpan.Length) return resultSpan[index];
        throw new InvalidDataException("Referenced unknown symbol");
    }
}