using System;
using System.Diagnostics;
using System.IO;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public static class HuffmanTableEvaluator
{
    public static int ReadHuffmanInt(this ref BitSource source, in ReadOnlySpan<HuffmanLine> lines)
    {
        var patternLen = 0;
        var pattern = 0;
        foreach (var line in lines)
        {
            Debug.Assert(patternLen <= line.PrefixLengh, "Lines must be ascending length order");
            while (patternLen < line.PrefixLengh) source.AddToPattern(ref pattern, ref patternLen);
            if (line.Matches(patternLen, pattern)) return line.ReadNum(ref source);
        }

        throw new InvalidDataException("Got to the end of a huffman table");
    }
}