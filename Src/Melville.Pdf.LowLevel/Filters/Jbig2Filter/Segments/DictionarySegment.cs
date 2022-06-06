using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class DictionarySegment: Segment
{
    public Memory<IBinaryBitmap> ExportedSymbols { get; }

    protected DictionarySegment(
        SegmentType type, Memory<IBinaryBitmap> exportedSymbols) : base(type)
    {
        ExportedSymbols = exportedSymbols;
    }
}