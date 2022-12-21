using System;
using System.Diagnostics;

namespace Melville.JBig2.HuffmanTables;

internal static class HuffmanTableFactory
{
    public static void FromIntSpan(in ReadOnlySpan<int> lengths, in Span<HuffmanLine> table)
    {
        Debug.Assert(lengths.Length == table.Length);
        Span<int> prefixes = stackalloc int[lengths.Length];
        BitAssignment.AssignPrefixes(lengths, prefixes);
        for (int i = 0; i < lengths.Length; i++)
        {
            table[i] = new HuffmanLine(lengths[i], prefixes[i], 0, i, 0);
        }
        table.Sort(CompareTableLines);
    }

    private static int CompareTableLines(HuffmanLine x, HuffmanLine y) => 
        (x.PrefixLengh, y.PrefixLengh) switch
        {
            (0,>0) => 1,
            (>0,0) => -1,
            var (xKey, yKey) => xKey - yKey 
        };
}