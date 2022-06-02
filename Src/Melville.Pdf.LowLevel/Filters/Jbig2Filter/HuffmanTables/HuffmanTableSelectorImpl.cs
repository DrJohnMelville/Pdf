
using System;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public static class HuffmanTableSelectorImpl
{
    public static HuffmanLine[] GetTableLines(this HuffmanTableSelection sel, ref ReadOnlySpan<Segment> segments)
    {
        if (sel is HuffmanTableSelection.UserSupplied)
            throw new NotImplementedException("Custom huffman table segments");
        return StandardHuffmanTables.ArrayFromSelector(sel);
    }
}