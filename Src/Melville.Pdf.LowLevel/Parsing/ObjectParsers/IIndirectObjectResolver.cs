using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public interface IIndirectObjectResolver
{
    IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects();
    PdfIndirectObject FindIndirect(int number, int generation);
    void AddLocationHint(PdfIndirectObject newItem);
    Task<long> FreeListHead();
}

public static class IndirectObjectResolverOperations
{
    public static void RegistedDeletedBlock(
        this IIndirectObjectResolver resolver, int number, int next, int generation) =>
        resolver.AddLocationHint(new IndirectObjectWithAccessor(number, generation,
            () => new ValueTask<PdfObject>(new PdfFreeListObject(next))));
    public static void RegistedNullObject(
        this IIndirectObjectResolver resolver, int number, int next, int generation) =>
        resolver.AddLocationHint(
            new PdfIndirectObject(number, generation,PdfTokenValues.Null));

    public static void RegisterIndirectBlock(
        this ParsingFileOwner owner, int number, long generation, long offset)
    {
        owner.IndirectResolver.AddLocationHint(new RawLocationIndirectObject(number, (int)generation,
            owner, offset));
    }
    public static void RegisterObjectStreamBlock(
        this ParsingFileOwner owner, int number, long referredStream, long referredOrdinal)
    {
        if (number == referredStream) throw new PdfParseException("A object stream may not contain itself");
        owner.IndirectResolver.AddLocationHint(new IndirectObjectWithAccessor(number, 0,
            async () =>
            {
                var referredObject = await owner.IndirectResolver
                    .FindIndirect((int)referredStream, 0).DirectValueAsync().CA();
                if (referredObject is not PdfStream stream) return PdfTokenValues.Null;
                return await LoadObjectStream(owner, stream, number).CA();
            }));
    }
        
    public static async ValueTask<PdfObject> LoadObjectStream(
        ParsingFileOwner owner, PdfStream source, int objectNumber)
    {
        PdfObject ret = PdfTokenValues.Null;
        await using var data = await source.StreamContentAsync().CA();
        var reader = owner.ParsingReaderForStream(data, 0);
        var objectLocations = await ObjectStreamOperations.GetIncludedObjectNumbers(
            source, reader.Reader).CA();
        var first = (await source.GetAsync<PdfNumber>(KnownNames.First).CA()).IntValue;
        foreach (var location in objectLocations)
        {
            await reader.Reader.Source.AdvanceToLocalPositionAsync(first + location.Offset).CA();
            var obj = await PdfParserParts.Composite.ParseAsync(reader).CA();
            if (objectNumber == location.ObjectNumber)
                ret = obj;
            AcceptObject(owner.IndirectResolver,location.ObjectNumber,obj);
        }

        return ret;
    }
        
    public static void AcceptObject(IIndirectObjectResolver resolver,
        int objectNumber, PdfObject pdfObject) =>
        ((IMultableIndirectObject)resolver.FindIndirect(objectNumber, 0)).SetValue(pdfObject);
}