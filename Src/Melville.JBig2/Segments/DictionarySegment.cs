using System;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.FileOrganization;

namespace Melville.JBig2.Segments;

public class DictionarySegment: Segment
{
    public Memory<IBinaryBitmap> ExportedSymbols { get; }

    protected DictionarySegment(
        SegmentType type, Memory<IBinaryBitmap> exportedSymbols) : base(type)
    {
        ExportedSymbols = exportedSymbols;
    }
}