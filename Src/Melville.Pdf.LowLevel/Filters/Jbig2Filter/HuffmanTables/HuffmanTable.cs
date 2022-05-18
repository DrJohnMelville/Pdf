using System;
using System.Linq;

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
        return source.ReadHuffmanInt(lines);
    }

    public bool HasOutOfBandRow() => lines.Any(i => i.IsOutOfBandRow);

    public bool IsOutOfBand(int value) => value == int.MaxValue;
}

public readonly ref struct StructHuffmanTable
{
    private readonly ReadOnlySpan<HuffmanLine> lines;

    public StructHuffmanTable(in ReadOnlySpan<HuffmanLine> lines)
    {
        this.lines = lines;
    }

    public int GetInteger(ref BitSource source)
    {
        return source.ReadHuffmanInt(lines);
    }
}