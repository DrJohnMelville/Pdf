using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

public readonly struct GenericRegionRefinementAlgorithm
{
    private readonly IBinaryBitmap target;
    private readonly IBinaryBitmap reference;
    private readonly bool useTypicalPredicition;
    private readonly RefinementTemplateSet template;
    private readonly MQDecoder decoder;

    public GenericRegionRefinementAlgorithm(
        IBinaryBitmap target, IBinaryBitmap reference, 
        bool useTypicalPredicition, in RefinementTemplateSet template, MQDecoder decoder)
    {
        this.target = target;
        this.reference = reference;
        this.useTypicalPredicition = useTypicalPredicition;
        this.template = template;
        this.decoder = decoder;
    }

    public void Read(ref SequenceReader<byte> source)
    {
        if (useTypicalPredicition)
            throw new NotImplementedException("Typical Prediction is not implemented");
        for (int i = 0; i < target.Height; i++)
        {
            for (int j = 0; j < target.Width; j++)
            { 
                ref var context = ref 
                    template.ContextFor(reference, target, i, j);
                var bit = decoder.GetBit(ref source, ref context);
                #warning -- need to use the operation here.
                target[i, j] = bit == 1;
            }
        }
    }
}