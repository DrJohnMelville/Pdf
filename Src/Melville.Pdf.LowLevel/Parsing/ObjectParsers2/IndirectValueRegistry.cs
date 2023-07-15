using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal class IndirectValueRegistry : IIndirectValueSource
{
    private readonly UnenclosedDeferredPdfStrategy unenclosedObjectStrategy;
    private readonly ObjectStreamDeferredPdfStrategy objectStreamStrategy;

    private readonly Dictionary<MementoUnion, PdfDirectValue> items = new();

    public IndirectValueRegistry(ParsingFileOwner owner)
    {
        unenclosedObjectStrategy = new(owner);
        objectStreamStrategy = new(this);
    }

    public PdfIndirectValue CreateReference(ulong item, ulong generation) =>
        new(this, NumberAndGenerationToMemento(item, generation));

    public string GetValue(in MementoUnion memento) =>
        $"{memento.UInt64s[0]} {memento.UInt64s[1]} R";

    public async ValueTask<PdfDirectValue> Lookup(MementoUnion memento)
    {
        var item = CollectionsMarshal.GetValueRefOrNullRef(items, memento);
        if (Unsafe.IsNullRef(ref item)) return PdfDirectValue.CreateNull();

        if (item.TryGet(out DeferredPdfHolder deferred))
        {
            item = await deferred.GetAsync().CA();
        }

        return item;
    }


    public void RegisterDirectObject(ulong number, ulong generation, in PdfDirectValue value) =>
        RegisterDirectObject(NumberAndGenerationToMemento(number, generation), value);

    private static MementoUnion NumberAndGenerationToMemento(ulong number, ulong generation) => 
        MementoUnion.CreateFrom(number, generation);

    public void RegisterDirectObject(in MementoUnion memento, in PdfDirectValue value) =>
        items[memento] = value;

    public void RegisterUnenclosedObject(ulong number, ulong generation, ulong offset) => 
        RegisterDirectObject(number, generation, unenclosedObjectStrategy.Create(offset));

    public void RegisterObjectStreamObject(ulong number, ulong streamNumber, ulong streamPosition) =>
        RegisterDirectObject(number, 0,
            objectStreamStrategy.Create(streamNumber, streamPosition));
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

    public PdfDirectValue Create(ulong offset)
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

    public PdfDirectValue Create(ulong streamNum, ulong streamPosition)
    {
        return new(this, MementoUnion.CreateFrom(streamNum, streamPosition));
    }
}