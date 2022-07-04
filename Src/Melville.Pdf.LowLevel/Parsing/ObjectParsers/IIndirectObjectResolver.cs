using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public interface IIndirectObjectResolver
{
    IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects();
    PdfIndirectObject FindIndirect(int number, int generation);
    void AddLocationHint(PdfIndirectObject newItem);
    long FreeListHead();
}
    
public static class IndirectObjectResolverOperations
{
    public static void RegistedDeletedBlock(
        this IIndirectObjectResolver resolver, int number, int next, int generation) =>
        resolver.AddLocationHint(new PdfFreeListObject(number, generation, next));

    public static void RegistedNullObject(
        this IIndirectObjectResolver resolver, int number, int generation) =>
        resolver.AddLocationHint(
            new PdfIndirectObject(number, generation,PdfTokenValues.Null));

    public static void RegisterIndirectBlock(
        this ParsingFileOwner owner, int number, long generation, long offset)
    {
        owner.IndirectResolver.AddLocationHint(new LocationIndirectObject(number, (int)generation, offset, owner));
    }

    public static void RegisterObjectStreamBlock(this ParsingFileOwner owner, int number, long referredStream)
    {
        if (number == referredStream) throw new PdfParseException("A object stream may not contain itself");
        owner.IndirectResolver.AddLocationHint(new ObjectStreamIndirectObject(number, 0, owner, referredStream));
    }
}