using System;
using System.IO;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public readonly ref struct CharacterDictionary
{
    private readonly ReadOnlySpan<Segment> dictionaries;

    public CharacterDictionary(ReadOnlySpan<Segment> dictionaries)
    {
        this.dictionaries = dictionaries;
    }
    
    public IBinaryBitmap GetBitmap(int index)
    {
        foreach (var segment in dictionaries)
        {
            if (segment is not DictionarySegment sds) continue;
            var exportedLength = sds.ExportedSymbols.Length;
            if (index < exportedLength) return sds.ExportedSymbols.Span[index];
            index -= exportedLength;
        }

        throw new InvalidDataException("Referenced unknown symbol");
    }

}