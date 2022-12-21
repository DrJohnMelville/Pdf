using System;
using Melville.JBig2.Segments;

namespace Melville.JBig2.HuffmanTables;

internal static class HuffmanTableSelectorImpl
{
    public static HuffmanLine[] GetTableLines(this HuffmanTableSelection sel, ref ReadOnlySpan<Segment> segments)
    {
        if (sel is HuffmanTableSelection.UserSupplied)
            throw new NotImplementedException("Custom huffman table segments");
        return StandardHuffmanTables.ArrayFromSelector(sel);
    }
}