using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;


internal class IndirectObjectRegistry : IIndirectObjectSource, IIndirectObjectRegistry
{
    private readonly UnenclosedDeferredPdfStrategy unenclosedObjectStrategy;
    private readonly ObjectStreamDeferredPdfStrategy objectStreamStrategy;

    private readonly Dictionary<(int, int), PdfIndirectObject> items = new();

    public IndirectObjectRegistry(ParsingFileOwner owner)
    {
        unenclosedObjectStrategy = new(owner);
        objectStreamStrategy = new(owner);
    }

    public PdfIndirectObject CreateReference(int item, int generation) =>
        new(this, PairToMemento(item, generation));

    public string GetValue(in MementoUnion memento) =>
        $"{memento.Int32s[0]} {memento.Int32s[1]} R";

    public ValueTask<PdfDirectObject> LookupAsync(MementoUnion memento) =>
        LookupAsync(MementoToPair(memento));

    public ValueTask<PdfDirectObject> LookupAsync(int objectNumber, int generation) =>
        LookupAsync((objectNumber, generation));

    private ValueTask<PdfDirectObject> LookupAsync((int ObjectNumber, int Generation) key)
    {
        if (!items.TryGetValue(key, out var item)) return new(PdfDirectObject.CreateNull());
        if (item.TryGetEmbeddedDirectValue(out var dv)) return new(dv);
        return ResolveIndirectValueAsync(key, item);
    }

    private async ValueTask<PdfDirectObject> ResolveIndirectValueAsync(
        (int ObjectNumber, int Generation) key, PdfIndirectObject item)
    {
        var ret = await item.LoadValueAsync().CA();
        items[key] = ret; // have to do the duplicate  lookup because it is an async method.
        return ret;
    }

    internal void RegisterDirectObject(
        int number, int generation, in PdfIndirectObject value, bool doNotOverwrite)
    {
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(items, (number, generation), out var previouslyDefined);
        if (previouslyDefined && doNotOverwrite ) return; 
        item = value;
    }

    public static MementoUnion PairToMemento(int number, int generation) => 
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
            offset, number, generation), true);

    public void RegisterObjectStreamBlock(
        int number, int referredStreamOrdinal, int positionInStream) => 
        RegisterDirectObject(number, 0, objectStreamStrategy.Create(
            referredStreamOrdinal, positionInStream, number), true);

    public IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects() => items;

}