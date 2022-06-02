using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public class HuffmanIntegerDecoder : EncodedReader<HuffmanLine[], BitReader>
{
    public HuffmanIntegerDecoder() : base(new BitReader())
    {
    }
    
    protected override int Read(ref SequenceReader<byte> source, HuffmanLine[] context)
    {
        var ret = source.ReadHuffmanInt(State, context.AsSpan());
        return ret;
    }

    public override bool IsOutOfBand(int item) => item == int.MaxValue;
    public override void ClearCommonContext() => State.DiscardPartialByte();
}