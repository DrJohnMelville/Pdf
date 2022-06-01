using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
[Obsolete("Going to IEncodedReader")]
public interface IIntegerDecoder
{
    int GetInteger(ref BitSource source);
    bool HasOutOfBandRow();
    bool IsOutOfBand(int value);
}

public class HuffmanTable : IIntegerDecoder
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

public static class HuffmanDebugSupport
{
    public static HuffmanLine[] VerifyHasOutOfBand(this HuffmanLine[] lines)
    {
        Debug.Assert(HasOutOfBand(lines));
        return lines;
    }
    public static HuffmanLine[] VerifyNoOutOfBand(this HuffmanLine[] lines)
    {
        Debug.Assert(!HasOutOfBand(lines));
        return lines;
    }

    private static bool HasOutOfBand(this HuffmanLine[] lines) => lines.Any(i=>i.IsOutOfBandRow);
}

public class HuffmanIntegerDecoder : EncodedReader<HuffmanLine[], BitReader>
{
    public HuffmanIntegerDecoder() : base(new BitReader())
    {
    }
    
    protected override int Read(ref SequenceReader<byte> source, HuffmanLine[] context, BitReader state)
    {
        var bitSource = new BitSource(source, state);
        var ret = bitSource.ReadHuffmanInt(context.AsSpan());
        source = bitSource.Source;
        return ret;
    }

    public override bool IsOutOfBand(int item) => item == int.MaxValue;
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