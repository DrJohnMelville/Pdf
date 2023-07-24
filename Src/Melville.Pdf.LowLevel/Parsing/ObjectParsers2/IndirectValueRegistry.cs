using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal class IndirectValueRegistry : IIndirectValueSource
{
    private readonly UnenclosedDeferredPdfStrategy unenclosedObjectStrategy;
    private readonly ObjectStreamDeferredPdfStrategy objectStreamStrategy;

    private readonly Dictionary<(int, int), PdfDirectValue> items = new();

    public IndirectValueRegistry(ParsingFileOwner owner)
    {
        unenclosedObjectStrategy = new(owner);
        objectStreamStrategy = new(this);
    }

    public PdfIndirectValue CreateReference(int item, int generation) =>
        new(this, PairToMemento(item, generation));

    public string GetValue(in MementoUnion memento) =>
        $"{memento.Int32s[0]} {memento.Int32s[1]} R";

    public async ValueTask<PdfDirectValue> Lookup(MementoUnion memento)
    {
        var item = CollectionsMarshal.GetValueRefOrNullRef(items, MementoToPair(memento));
        if (Unsafe.IsNullRef(ref item)) return PdfDirectValue.CreateNull();

        if (item.TryGet(out DeferredPdfHolder deferred))
        {
            item = await deferred.GetAsync().CA();
        }

        return item;
    }


    public void RegisterDirectObject(int number, int generation, in PdfDirectValue value) =>
        items[(number,generation)] = value;

    private static MementoUnion PairToMemento(int number, int generation) => 
        MementoUnion.CreateFrom(number, generation);

    private static (int number, int generation) MementoToPair(in MementoUnion memento)
    {
        var ints = memento.Int32s;
        return (ints[0], ints[1]);
    }

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        (objectNumber, generation) = MementoToPair(memento);
        return true;
    }

    public void RegisterDirectObject(in MementoUnion memento, in PdfDirectValue value) =>
        items[MementoToPair(memento)] = value;

    public void RegisterUnenclosedObject(int number, int generation, long offset) => 
        RegisterDirectObject(number, generation, unenclosedObjectStrategy.Create(offset));

    public void RegisterObjectStreamObject(int number, int streamNumber, int streamPosition) =>
        RegisterDirectObject(number, 0,
            objectStreamStrategy.Create(streamNumber, streamPosition));

    public IReadOnlyDictionary<(int, int), PdfIndirectValue> GetObjects()
    {
        return new IndirectRegistryWrapper<(int, int)>(items);
    }
}

internal interface IDeferredPdfObject: IPostscriptValueStrategy<DeferredPdfHolder>
{
    public ValueTask<PdfDirectValue> GetValue(MementoUnion memento);

    DeferredPdfHolder IPostscriptValueStrategy<DeferredPdfHolder>.GetValue(
        in MementoUnion memento) => new DeferredPdfHolder(this, memento);
}

internal readonly partial struct DeferredPdfHolder
{
    [FromConstructor] private readonly IDeferredPdfObject strategy;
    [FromConstructor] private readonly MementoUnion memento;

    public ValueTask<PdfDirectValue> GetAsync() => strategy.GetValue(memento);
}

internal partial class UnenclosedDeferredPdfStrategy : IDeferredPdfObject
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    public ValueTask<PdfDirectValue> GetValue(MementoUnion memento)
    {
        throw new NotImplementedException("Need to Read a root pdf value");
    }

    public PdfDirectValue Create(long offset)
    {
        return new(this, MementoUnion.CreateFrom(offset));
    }
}

internal partial class ObjectStreamDeferredPdfStrategy : IDeferredPdfObject
{
    [FromConstructor] private readonly IndirectValueRegistry owner;

    public ValueTask<PdfDirectValue> GetValue(MementoUnion memento)
    {
        throw new NotImplementedException("Need to Read ObjectStrea pdf values");
    }

    public PdfDirectValue Create(int streamNum, int streamPosition)
    {
        return new(this, MementoUnion.CreateFrom(streamNum, streamPosition));
    }
}