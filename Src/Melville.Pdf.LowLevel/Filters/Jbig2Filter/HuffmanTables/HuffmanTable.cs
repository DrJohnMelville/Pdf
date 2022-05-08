
using System.Diagnostics;
using System.IO;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public class HuffmanTable
{
    private readonly HuffmanLine[] lines;

    public HuffmanTable(params HuffmanLine[] lines)
    {
        this.lines = lines;
    }

    public int GetInteger(ref BitSource source)
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