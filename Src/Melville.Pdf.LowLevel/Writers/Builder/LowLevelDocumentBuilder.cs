using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class LowLevelDocumentBuilder : ILowLevelDocumentBuilder
{
    private int nextObject;
    public List<PdfIndirectObject> Objects { get;  }= new();
    private readonly DictionaryBuilder trailerDictionaryItems = new();
    private ObjectStreamBuilder? objectStreamBuilder;
        
    public string UserPassword { get; set; } = "";

    public LowLevelDocumentBuilder(int nextObject = 1)
    {
        this.nextObject = nextObject;
    }
        
    public PdfIndirectObject AsIndirectReference(PdfObject value) =>
        value switch
        {
            PdfIndirectObject pio => pio,
            null=> throw new ArgumentException("To create unbound indirect objects use CreatePromiseObject.  Value cannot be null."),
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
        trailerDictionaryItems.WithItem(key,  item);

    public PdfDictionary CreateTrailerDictionary()
    {
        AddLengthToTrailerDictionary();
        return trailerDictionaryItems.AsDictionary();
    }
    private void AddLengthToTrailerDictionary()
    {
        AddToTrailerDictionary(KnownNames.Size, nextObject);
    }

    public PdfArray EnsureDocumentHasId() =>
        trailerDictionaryItems.TryGetValue(KnownNames.ID, out var val) && val is PdfArray ret
            ? ret
            : AddNewIdArray();

    private PdfArray AddNewIdArray()
    {
        var array = NewIdArray();
        AddToTrailerDictionary(KnownNames.ID, array);
        return array;
    }

    private PdfArray NewIdArray() => new(IdElement(), IdElement());

    private PdfString IdElement()
    {
        var ret = new byte[32];
        Guid.NewGuid().TryWriteBytes(ret);
        Guid.NewGuid().TryWriteBytes(ret.AsSpan(16));
        return new PdfString(ret);
    }

    public IDisposable ObjectStreamContext(DictionaryBuilder? dictionaryBuilder = null)
    {
        if (objectStreamBuilder != null)
            throw new InvalidOperationException("Cannot nest object stream contents");
        objectStreamBuilder = new ObjectStreamBuilder();
        return new ObjectStreamContextImpl(this, 
            ExpandDefaultBuilder(dictionaryBuilder));
    }

    private static DictionaryBuilder ExpandDefaultBuilder(DictionaryBuilder? dictionaryBuilder) => 
        dictionaryBuilder?? new DictionaryBuilder().WithFilter(FilterName.FlateDecode);

    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7)
    {
        return new PdfLowLevelDocument(major, minor, CreateTrailerDictionary(), Objects.ToDictionary(
            static item => (item.ObjectNumber, item.GenerationNumber) ));
    }
}