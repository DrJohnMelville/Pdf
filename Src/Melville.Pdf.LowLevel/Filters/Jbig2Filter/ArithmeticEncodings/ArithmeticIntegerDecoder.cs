using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
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
        context.ShouldUseTypicalAlgorithm ?
        new TypicalIntegerDecoder(context, State).Read(ref source):
        new SymbolIntegerDecoder(State, context).Read(ref source);

    public override void ReadBitmap(ref SequenceReader<byte> source, BinaryBitmap target) => 
        new AritmeticBitmapReader(target, State, template, 0, false).Read(ref source);

    public override void PrepareForRefinementSymbolDictionary(uint totalSymbols)
    {
        var bits = IntLog.CeilingLog2Of(totalSymbols);
        SymbolIdContext = new ContextStateDict(bits, bits);
        RefinementXContext = new ContextStateDict(9);
        RefinementYContext = new ContextStateDict(9);
    }

    public override void InvokeSymbolRefinement(BinaryBitmap destination, IBinaryBitmap reference, int deltaX, int deltaY,
        bool useTypicalPrediction, in RefinementTemplateSet refinementTemplate, ref SequenceReader<byte> source)
    {
        new GenericRegionRefinementAlgorithm(destination, reference, deltaX, deltaY, useTypicalPrediction,
            refinementTemplate, State).Read(ref source);
    }
}