using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.HalftoneRegionParsers;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public class ArithmeticIntegerDecoder: EncodedReader<ContextStateDict, MQDecoder>
{
    private readonly ArithmeticBitmapReaderContext template;

    public ArithmeticIntegerDecoder(ArithmeticBitmapReaderContext template) : base(new MQDecoder())
    {
        this.template = template;
    }

    public override bool IsOutOfBand(int item) => item == int.MaxValue;

    protected override int Read(ref SequenceReader<byte> source, ContextStateDict context) =>
        new TypicalIntegerDecoder(context, State).Read(ref source);
        

    protected override int ReadSymbol(ref SequenceReader<byte> source, ContextStateDict context) =>
        new SymbolIntegerDecoder(State, context).Read(ref source);

    public override void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target) => 
        new ArithmeticGenericRegionDecodeProcedure(target, State, template, 0, DoNotSkip.Instance).Read(ref source);

    public override void PrepareForRefinementSymbolDictionary(uint totalSymbols)
    {
        var bits = IntLog.CeilingLog2Of(totalSymbols);
        SymbolIdContext = new ContextStateDict(bits);
        FirstSContext = new ContextStateDict(9);
        DeltaSContext = new ContextStateDict(9);
        DeltaTContext = new ContextStateDict(9);
        RefinementDeltaWidthContext = new ContextStateDict(9);
        RefinementDeltaHeightContext = new ContextStateDict(9);
        RefinementXContext = new ContextStateDict(9);
        RefinementYContext = new ContextStateDict(9);
        RefinementSizeContext = new ContextStateDict(9);
        RIBitContext = new ContextStateDict(9);
    }

    public override void InvokeSymbolRefinement(IBinaryBitmap destination, IBinaryBitmap reference,
        int typicalPredictionContext, in RefinementTemplateSet refinementTemplate, 
        ref SequenceReader<byte> source) =>
        new GenericRegionRefinementAlgorithm(destination, reference, refinementTemplate, State,
            typicalPredictionContext).Read(ref source);
}