using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        Debug.Assert(referredOrdinal == 0); // assumed by this implementation
        owner.IndirectResolver.AddLocationHint(new ObjectStreamIndirectObject(
            number, 0, owner, referredStream));
    }
}