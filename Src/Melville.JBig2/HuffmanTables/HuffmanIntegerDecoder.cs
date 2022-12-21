using System;
using System.Buffers;
using System.Diagnostics;
using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.EncodedReaders;
using Melville.JBig2.GenericRegionRefinements;
using Melville.JBig2.SegmentParsers.HalftoneRegionParsers;
using Melville.Parsing.VariableBitEncoding;

namespace Melville.JBig2.HuffmanTables;

internal class HuffmanIntegerDecoder : EncodedReader<HuffmanLine[], BitReader>
{
    public HuffmanIntegerDecoder() : base(new BitReader())
    {
    }
    
    protected override int Read(ref SequenceReader<byte> source, HuffmanLine[] context) => 
        source.ReadHuffmanInt(State, context.AsSpan());

    protected override int ReadSymbol(ref SequenceReader<byte> source, HuffmanLine[] context) =>
        Read(ref source, context);

    public override bool IsOutOfBand(int item) => item == int.MaxValue;
    
    public override void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target)
    {
        var bitmapLength = BitmapSize(ref source);
        State.DiscardPartialByte();
        if (bitmapLength == 0)
            target.ReadUnencodedBitmap(ref source);
        else
            target.ReadMmrEncodedBitmap(ref source, false);
    }

    public override void PrepareForRefinementSymbolDictionary(uint totalSymbols)
    {
        SymbolIdContext = DirectBitstreamReaders.FromBitLength(IntLog.CeilingLog2Of(totalSymbols));
        FirstSContext = StandardHuffmanTables.B6;
        DeltaSContext = StandardHuffmanTables.B8;
        DeltaTContext = StandardHuffmanTables.B11;
        RefinementDeltaWidthContext = StandardHuffmanTables.B15;
        RefinementDeltaHeightContext = StandardHuffmanTables.B15;
        RefinementXContext = RefinementYContext = StandardHuffmanTables.B15;
        RefinementYContext = RefinementYContext = StandardHuffmanTables.B15;
        RefinementSizeContext = StandardHuffmanTables.B1;
 //     this is a potential bug where the symbol header and the refinement algorithm specify different decoders.");
        Debug.Assert(BitmapSizeContext == null || BitmapSizeContext == StandardHuffmanTables.B1);
        BitmapSizeContext = StandardHuffmanTables.B1;
        RIBitContext = DirectBitstreamReaders.OneBit;
    }

    public override void InvokeSymbolRefinement(
        IBinaryBitmap destination, IBinaryBitmap reference,
        int predictionContext, scoped in RefinementTemplateSet refinementTemplate,
        scoped ref SequenceReader<byte> source)
    {
        var datalen = BitmapSize(ref source);
        State.DiscardPartialByte();
        new GenericRegionRefinementAlgorithm(destination, reference, refinementTemplate, new MQDecoder(),
            predictionContext).Read(ref source);
        // implicitly discard any partial bytes left over in the MQDecoder.
    }
}