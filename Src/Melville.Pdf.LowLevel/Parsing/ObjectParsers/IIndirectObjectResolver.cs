using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public interface IIndirectObjectResolver
{
    IReadOnlyDictionary<(int, int), PdfIndirectReference> GetObjects();
    PdfIndirectReference FindIndirect(int number, int generation);
    void AddLocationHint(int number, int generation, Func<ValueTask<PdfObject>> valueAccessor);
    Task<long> FreeListHead();
}
    
public static class IndirectObjectResolverOperations
{
    public static void RegistedDeletedBlock(
        this IIndirectObjectResolver resolver, int number, int next, int generation) =>
        resolver.AddLocationHint(number, generation,
            () => new ValueTask<PdfObject>(new PdfFreeListObject(next)));
    public static void RegistedNullObject(
        this IIndirectObjectResolver resolver, int number, int next, int generation) =>
        resolver.AddLocationHint(number, generation,
            () => new ValueTask<PdfObject>(PdfTokenValues.Null));

    public static void RegisterIndirectBlock(
        this ParsingFileOwner owner, int number, long generation, long offset)
    {
        owner.IndirectResolver.AddLocationHint(number, (int)generation,
            async () =>
            {
                var rentedReader = await owner.RentReader(offset, number, (int)generation);
                return await rentedReader.RootObjectParser.ParseAsync(rentedReader);
            });
    }
    public static void RegisterObjectStreamBlock(
        this ParsingFileOwner owner, int number, long referredStream, long referredOrdinal)
    {
        if (number == referredStream) throw new PdfParseException("A object stream may not contain itself");
        owner.IndirectResolver.AddLocationHint(number, 0,
            async () =>
            {
                var referredObject = await owner.IndirectResolver
                    .FindIndirect((int)referredStream, 0).DirectValueAsync();
                if (referredObject is not PdfStream stream) return PdfTokenValues.Null;
                return await LoadObjectStream(owner, stream, number);
            });
    }
        
    public static async ValueTask<PdfObject> LoadObjectStream(
        ParsingFileOwner owner, PdfStream source, int objectNumber)
    {
        PdfObject ret = PdfTokenValues.Null;
        await using var data = await source.StreamContentAsync();
        var reader = owner.ParsingReaderForStream(data, 0);
        var objectLocations = await ObjectStreamOperations.GetIncludedObjectNumbers(
            source, reader.Reader);
        var first = (await source.GetAsync<PdfNumber>(KnownNames.First)).IntValue;
        foreach (var location in objectLocations)
        {
            await reader.Reader.AdvanceToLocalPositionAsync(first + location.Offset);
            var obj = await PdfParserParts.Composite.ParseAsync(reader);
            if (objectNumber == location.ObjectNumber)
                ret = obj;
            AcceptObject(owner.IndirectResolver,location.ObjectNumber,obj);
        }

        return ret;
    }
        
    public static void AcceptObject(IIndirectObjectResolver resolver,
        int objectNumber, PdfObject pdfObject) =>
        ((IMultableIndirectObject)resolver.FindIndirect(objectNumber, 0).Target).SetValue(pdfObject);


}