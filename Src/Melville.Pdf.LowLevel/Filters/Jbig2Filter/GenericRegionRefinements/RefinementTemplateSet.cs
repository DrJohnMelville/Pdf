using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

public readonly struct RefinementTemplateSet
{
    private readonly BitmapTemplate referenceTemplate;
    private readonly BitmapTemplate destinationTemplate;
    private readonly ContextStateDict contextDictionary;

    public RefinementTemplateSet(ref SequenceReader<byte> source, bool useTemplate1)
    {
        var referenceFactory = new BitmapTemplateFactory(
            useTemplate1 ? GenericRegionTemplate.RefinementReference1 : GenericRegionTemplate.RefinementReference0);
        var destinationFactory = new BitmapTemplateFactory(
            useTemplate1 ? GenericRegionTemplate.RefinementDestination1 : GenericRegionTemplate.RefinementDestination0);
        if (!useTemplate1)
        {
            REadAdaptivePixels(ref source, ref destinationFactory, ref referenceFactory);
        }

        referenceTemplate = referenceFactory.Create();
        destinationTemplate = destinationFactory.Create();
        contextDictionary = new ContextStateDict(referenceTemplate.BitsRequired() + destinationTemplate.BitsRequired());
    }

    private static void REadAdaptivePixels(
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

    public ref ContextEntry ContextFor(IBinaryBitmap reference, int refRow, int refCol,
        IBinaryBitmap destination, int destRow, int destCol) =>
        ref contextDictionary.EntryForContext(
            ComputeCompositeContext(reference, refRow, refCol, destination, destRow, destCol));

    private ushort ComputeCompositeContext(IBinaryBitmap reference, int refRow, int refCol, 
        IBinaryBitmap destination, int destRow, int destCol) =>
        (ushort)(
            (referenceTemplate.ReadContext(reference, refRow, refCol) << destinationTemplate.BitsRequired()) |
            destinationTemplate.ReadContext(destination, destRow, destCol));
}