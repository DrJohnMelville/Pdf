using System;
using System.Buffers;
using Melville.JBig2.ArithmeticEncodings;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.GenericRegionRefinements;

internal readonly struct RefinementTemplateSet
{
    private readonly ContextBitRun[] runs;
    private readonly ContextStateDict contextDictionary;

    public RefinementTemplateSet(ref SequenceReader<byte> source, bool useTemplate1)
    {
        var referenceFactory = new BitmapTemplateFactory(
            useTemplate1 ? GenericRegionTemplate.RefinementReference1 : GenericRegionTemplate.RefinementReference0);
        var destinationFactory = new BitmapTemplateFactory(
            useTemplate1 ? GenericRegionTemplate.RefinementDestination1 : GenericRegionTemplate.RefinementDestination0);
        if (!useTemplate1)
        {
            ReadAdaptivePixels(ref source, ref destinationFactory, ref referenceFactory);
        }

        var referenceRunLength = referenceFactory.Length;
        var destinationRunLength = destinationFactory.Length;

        runs = new ContextBitRun[referenceRunLength + destinationRunLength];
        referenceFactory.WriteRunsToSpan(runs.AsSpan());
        destinationFactory.WriteRunsToSpan(runs.AsSpan(referenceRunLength));

        contextDictionary = new ContextStateDict(runs[0].NextBit() + runs[referenceRunLength].NextBit());
    }

    private static void ReadAdaptivePixels(
        ref SequenceReader<byte> source, ref BitmapTemplateFactory destinationFactory,
        ref BitmapTemplateFactory referenceFactory)
    {
        var atX1 = source.ReadBigEndianInt8();
        var atY1 = source.ReadBigEndianInt8();
        var atX2 = source.ReadBigEndianInt8();
        var atY2 = source.ReadBigEndianInt8();
        destinationFactory.AddPoint(atY1, atX1);
        referenceFactory.AddPoint(atY2, atX2);
    }

    public ref ContextEntry ContextFor(int context) => ref contextDictionary.EntryForContext(context);

    public IncrementalTemplate ToIncrementalTemplate() => new(runs);
}