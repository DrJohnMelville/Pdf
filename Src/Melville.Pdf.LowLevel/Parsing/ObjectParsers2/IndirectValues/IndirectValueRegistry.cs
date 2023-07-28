using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2.IndirectValues;


internal class IndirectValueRegistry : IIndirectValueSource, IIndirectObjectRegistry
{
    private readonly UnenclosedDeferredPdfStrategy unenclosedObjectStrategy;
    private readonly ObjectStreamDeferredPdfStrategy objectStreamStrategy;

    private readonly Dictionary<(int, int), PdfIndirectValue> items = new();

    public IndirectValueRegistry(ParsingFileOwner owner)
    {
        unenclosedObjectStrategy = new(owner);
        objectStreamStrategy = new(owner);
    }

    public PdfIndirectValue CreateReference(int item, int generation) =>
        new(this, PairToMemento(item, generation));

    public string GetValue(in MementoUnion memento) =>
        $"{memento.Int32s[0]} {memento.Int32s[1]} R";

    public ValueTask<PdfDirectValue> LookupAsync(MementoUnion memento) =>
        LookupAsync(MementoToPair(memento));

    public ValueTask<PdfDirectValue> LookupAsync(int objectNumber, int generation) =>
        LookupAsync((objectNumber, generation));

    private ValueTask<PdfDirectValue> LookupAsync((int ObjectNumber, int Generation) key)
    {
        if (!items.TryGetValue(key, out var item)) return new(PdfDirectValue.CreateNull());
        if (item.TryGetEmbeddedDirectValue(out var dv)) return new(dv);
        return ResolveIndirectValueAsync(key, item);
    }

    private async ValueTask<PdfDirectValue> ResolveIndirectValueAsync(
        (int ObjectNumber, int Generation) key, PdfIndirectValue item)
    {
        var ret = await item.LoadValueAsync().CA();
        items[key] = ret; // have to do the duplicate  lookup because it is an async method.
        return ret;
    }

    internal void RegisterDirectObject(
        int number, int generation, in PdfIndirectValue value, bool doNotOverwrite)
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

    public IReadOnlyDictionary<(int, int), PdfIndirectValue> GetObjects() => items;

}