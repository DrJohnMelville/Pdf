using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class PdfObjectRegistry: 
    IPdfObjectCreatorRegistry, IIndirectObjectSource
{
    [FromConstructor] private int nextObject;
    
    public  Dictionary<(int, int), PdfDirectObject> Objects { get; } = new();
    public DictionaryBuilder TrailerDictionaryItems { get; } = new();
    private ObjectStreamBuilder? objectStreamBuilder;

    public PdfIndirectObject Add(in PdfDirectObject item) =>
        Add(item, nextObject++, 0);

    public PdfIndirectObject Add(in PdfDirectObject item, int objectNumber, int generation)
    {
        if (!TryWriteToObjectStream(item, objectNumber, generation))
        {
            Objects[(objectNumber, generation)] = item;
        }

        return new PdfIndirectObject(this, MementoUnion.CreateFrom(objectNumber, generation));
    }

    private bool TryWriteToObjectStream(
        in PdfDirectObject item, int objectNumber, int generation) =>
        objectStreamBuilder is not null && generation == 0 &&
        objectStreamBuilder.TryAddRef(objectNumber, item);

    public void Reassign(in PdfIndirectObject item, in PdfDirectObject newValue)
    {
        var ints = item.Memento.Int32s;
        Objects[(ints[0],ints[1])] = newValue;
    }

    public void AddToTrailerDictionary(in PdfDirectObject key, in PdfIndirectObject item) => 
        TrailerDictionaryItems.WithItem(key, item);

    public IDisposable ObjectStreamContext(
        DictionaryBuilder? dictionaryBuilder = null)
    {
        if (objectStreamBuilder != null)
            throw new InvalidOperationException("Cannot nest object stream builders");
        objectStreamBuilder = new ObjectStreamBuilder(dictionaryBuilder);
        return new DisposeToFinishStream(this);
    }

    public partial class DisposeToFinishStream: IDisposable
    {
        [FromConstructor]private readonly PdfObjectRegistry registry;

        public void Dispose()
        {
            var osb = registry.objectStreamBuilder;
            registry.objectStreamBuilder = null;
            if (osb.HasValues())
                registry.Add(new PdfDirectObject(osb, default));
        }
    }



    public string GetValue(in MementoUnion memento)
    {
        if (!TryGetObjectReference(out var obj, out var gen, memento))
            throw new InvalidOperationException("Find reference for direct object");
        return $"{obj} {gen} R";
    }

    public ValueTask<PdfDirectObject> LookupAsync(MementoUnion memento)
    {
        if (!TryGetObjectReference(out var obj, out var gen, memento))
            throw new InvalidOperationException("Find reference for direct object");
        return ValueTask.FromResult(Objects[(obj,gen)]);
    }

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        var ints = memento.Int32s;
        objectNumber = ints[0];
        generation = ints[1];
        return true;
    }

    public PdfDictionary CreateTrailerDictionary() =>
        TrailerDictionaryItems
            .WithItem(KnownNames.SizeTName, nextObject)
            .AsDictionary();
}