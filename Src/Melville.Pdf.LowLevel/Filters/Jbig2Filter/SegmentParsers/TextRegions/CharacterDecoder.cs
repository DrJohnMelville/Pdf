using System;
using System.IO;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;

public readonly ref struct CharacterDecoder
{
    private readonly IIntegerDecoder decoder;
    private readonly ReadOnlySpan<Segment> segments;

    public CharacterDecoder(IIntegerDecoder decoder, ReadOnlySpan<Segment> segments)
    {
        this.decoder = decoder;
        this.segments = segments;
    }

    public IBinaryBitmap GetBitmap(ref BitSource source)
    {
        var index = decoder.GetInteger(ref source);
        foreach (var segment in segments)
        {
            if (segment is not SymbolDictionarySegment sds) continue;
            var exportedLength = sds.ExportedSymbols.Length;
            if (index < exportedLength) return sds.ExportedSymbols.Span[index];
            index -= exportedLength;
        }

        throw new InvalidDataException("Referenced unknown symbol");
    }
}