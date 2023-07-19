using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class PdfObjectRegistry: 
    IPdfObjectCreatorRegistry, IIndirectValueSource
{
    [FromConstructor] private int nextObject;
    
    public  Dictionary<(int, int), PdfDirectValue> Objects { get; } = new();
    public ValueDictionaryBuilder TrailerDictionaryItems { get; } = new();
    private ObjectStreamBuilder? objectStreamBuilder;

    public PdfIndirectValue Add(in PdfDirectValue item) =>
        Add(item, nextObject++, 0);

    public PdfIndirectValue Add(in PdfDirectValue item, int objectNumber, int generation)
    {
        if (!TryWriteToObjectStream(item, objectNumber, generation))
        {
            Objects[(objectNumber, generation)] = item;
        }

        return new PdfIndirectValue(this, MementoUnion.CreateFrom(objectNumber, generation));
    }

    private bool TryWriteToObjectStream(
        in PdfDirectValue item, int objectNumber, int generation) =>
        objectStreamBuilder is not null && generation == 0 &&
        !objectStreamBuilder.TryAddRef(objectNumber, item);

    public void Reassign(in PdfIndirectValue item, in PdfDirectValue newValue)
    {
        var ints = item.Memento.Int32s;
        Objects[(ints[0],ints[1])] = newValue;
    }

    public void AddToTrailerDictionary(in PdfDirectValue key, in PdfIndirectValue item) => 
        TrailerDictionaryItems.WithItem(key, item);

    public IDisposable ObjectStreamContext(
        ValueDictionaryBuilder? dictionaryBuilder = null)
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
                registry.Add(new PdfDirectValue(osb, default));
        }
    }



    public string GetValue(in MementoUnion memento)
    {
        var nums = memento.Int32s;
        return $"{nums[0]} {nums[1]} R";
    }

    public ValueTask<PdfDirectValue> Lookup(MementoUnion memento)
    {
        var ints = memento.Int32s;
        return ValueTask.FromResult(Objects[((ints[0], ints[1]))]);
    }

    public PdfValueDictionary CreateTrailerDictionary() =>
        TrailerDictionaryItems
            .WithItem(KnownNames.SizeTName, nextObject)
            .AsDictionary();
}