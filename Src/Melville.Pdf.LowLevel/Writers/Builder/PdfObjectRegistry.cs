using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class PdfObjectRegistry: IPdfObjectRegistry
{
    [FromConstructor] private int nextObject;
    public  List<PdfIndirectObject> Objects { get; } = new();
    public DictionaryBuilder TrailerDictionaryItems { get; } = new();
    private ObjectStreamBuilder? objectStreamBuilder;

    public PdfIndirectObject AsIndirectReference(PdfObject value) =>
        value switch
        {
            PdfIndirectObject pio => pio,
            null => throw new ArgumentException("To create unbound indirect objects use CreatePromiseObject.  Value cannot be null."),
            _ => new PdfIndirectObject(nextObject++, 0, value)
        };

    public PromisedIndirectObject CreatePromiseObject() => new(nextObject++, 0);


    public void AssignValueToReference(PdfIndirectObject reference, PdfObject value)
    {
        ((PromisedIndirectObject)reference).SetValue(value);
    }
    public PdfIndirectObject Add(PdfObject item) => InnerAdd(AsIndirectReference(item));

    public PdfIndirectObject Add(PdfObject item, int objectNumber, int generation) =>
        InnerAdd(new PdfIndirectObject(objectNumber, generation, item));

    public void AddDelayedObject(Func<ValueTask<PdfObject>> creator)
    {
        var reference = new IndirectObjectWithAccessor(nextObject++, 0, creator);
        Objects.Add(reference);
    }

    private PdfIndirectObject InnerAdd(PdfIndirectObject item)
    {
        if (!(Objects.Contains(item) || TryWriteToObjectStream(item)))
        {
            Objects.Add(item);
        }
        return item;
    }

    private bool TryWriteToObjectStream(PdfIndirectObject item) =>
        objectStreamBuilder is not null && objectStreamBuilder.TryAddRef(item);

    public void AddToTrailerDictionary(PdfName key, PdfObject item) =>
        TrailerDictionaryItems.WithItem(key, item);

    public PdfDictionary CreateTrailerDictionary()
    {
        AddLengthToTrailerDictionary();
        return TrailerDictionaryItems.AsDictionary();
    }
    private void AddLengthToTrailerDictionary()
    {
        AddToTrailerDictionary(KnownNames.Size, nextObject);
    }

    public IDisposable ObjectStreamContext(DictionaryBuilder? dictionaryBuilder = null)
    {
        if (objectStreamBuilder != null)
            throw new InvalidOperationException("Cannot nest object stream contents");
        objectStreamBuilder = new ObjectStreamBuilder();
        return new ObjectStreamContextImpl(this, DefaultBuilderIfNull(dictionaryBuilder));
    }
    private static DictionaryBuilder DefaultBuilderIfNull(DictionaryBuilder? dictionaryBuilder) =>
        dictionaryBuilder ?? new DictionaryBuilder().WithFilter(FilterName.FlateDecode);

}