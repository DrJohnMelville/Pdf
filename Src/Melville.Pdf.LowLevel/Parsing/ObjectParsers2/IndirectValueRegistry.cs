using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;


internal class IndirectValueRegistry : IIndirectValueSource, IIndirectObjectRegistry
{
    private readonly UnenclosedDeferredPdfStrategy unenclosedObjectStrategy;
    private readonly ObjectStreamDeferredPdfStrategy objectStreamStrategy;

    private readonly Dictionary<(int, int), PdfIndirectValue> items = new();

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
        var key = MementoToPair(memento);
        if (!items.TryGetValue(key, out var item)) return PdfDirectValue.CreateNull();

        if (item.TryGetEmbeddedDirectValue(out var dv)) return dv;

        var ret = await item.LoadValueAsync().CA();
        items[key] = ret; // have to do the duplicate  lookup because it is an async method.
        return ret;
    }

    private void RegisterDirectObject(int number, int generation, in PdfIndirectValue value)
    {
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(items, (number, generation), out var previouslyDefined);
        if (previouslyDefined) return; 
        item = value;
    }

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


    public void RegisterDeletedBlock(int number, int next, int generation)
    {
    }

    public void RegisterIndirectBlock(int number, int generation, long offset)=> 
        RegisterDirectObject(number, generation, unenclosedObjectStrategy.Create(
            offset, number, generation));

    public void RegisterObjectStreamBlock(
        int number, int referredStreamOrdinal, int positionInStream) => 
        RegisterDirectObject(number, 0, objectStreamStrategy.Create(
            referredStreamOrdinal, positionInStream));

    public IReadOnlyDictionary<(int, int), PdfIndirectValue> GetObjects() => items;
}

internal partial class UnenclosedDeferredPdfStrategy : IIndirectValueSource
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    public string GetValue(in MementoUnion memento) => $"Raw Offset Reference @{memento.UInt64s[1]}";

    public async ValueTask<PdfDirectValue> Lookup(MementoUnion memento)
    {
        var objectNumber = memento.Int32s[0];
        var generation = memento.Int32s[1];
        var offset = memento.Int64s[1];
        var reader = await owner.RentReaderAsync(offset, objectNumber, generation).CA();
        var result = await reader.NewRootObjectParser.ParseTopLevelObject().CA();
        return result;
    }

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        objectNumber = memento.Int32s[0];
        generation = memento.Int32s[1];
        return true;
    }

    public PdfIndirectValue Create(long offset, int number, int generation) => new(this, MementoUnion.CreateFrom(number, generation, offset));
}

internal partial class ObjectStreamDeferredPdfStrategy : IIndirectValueSource
{
    [FromConstructor] private readonly IndirectValueRegistry owner;

    public string GetValue(in MementoUnion memento) =>
        $"Load from Object Stream # {memento.Int32s[0]} as position {memento.Int32s[1]}";

    public ValueTask<PdfDirectValue> Lookup(MementoUnion memento)
    {
        throw new NotImplementedException("Load object from object stream not implemented");
    }

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        objectNumber = generation = -1;
        return false;
    }

    public PdfIndirectValue Create(int streamNum, int streamPosition) => 
        new(this, MementoUnion.CreateFrom(streamNum, streamPosition));
}