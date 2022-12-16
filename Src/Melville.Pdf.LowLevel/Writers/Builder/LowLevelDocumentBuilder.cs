using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentBuilder
{
    PdfIndirectObject AsIndirectReference(PdfObject? value = null);
    void AssignValueToReference(PdfIndirectObject reference, PdfObject value);
    PdfIndirectObject Add(PdfObject? item);
    public PdfIndirectObject Add(PdfObject item, int objectNumber, int generation);
    void AddToTrailerDictionary(PdfName key, PdfObject item);
    public PdfArray EnsureDocumentHasId();
    public string UserPassword { get; set; }
    IDisposable ObjectStreamContext(DictionaryBuilder? dictionaryBuilder = null);
}

public static class LowLevelDocumentBuilderOperations
{
    public static void AddRootElement(
        this ILowLevelDocumentBuilder creator, PdfDictionary rootElt) =>
        creator.AddToTrailerDictionary(KnownNames.Root, creator.Add(rootElt));
}

public static class BuildEncryptedDocument
{
    public static void AddEncryption(
        this ILowLevelDocumentBuilder builder,ILowLevelDocumentEncryptor encryptor)
    {
        builder.UserPassword = encryptor.UserPassword;
        builder.AddToTrailerDictionary(
            KnownNames.Encrypt,
            builder.Add(
                encryptor.CreateEncryptionDictionary(builder.EnsureDocumentHasId())));
    }
        
}
    
public class LowLevelDocumentBuilder : ILowLevelDocumentBuilder
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
        
    public PdfIndirectObject AsIndirectReference(PdfObject? value = null) =>
        value switch
        {
            PdfIndirectObject pio => pio,
            null=> new PromisedIndirectObject(nextObject++, 0),
            _ => new PdfIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null)
        };

    public void AssignValueToReference(PdfIndirectObject reference, PdfObject value)
    {
        ((PromisedIndirectObject)reference).SetValue(value);
    }
    public PdfIndirectObject Add(PdfObject? item) => InnerAdd(AsIndirectReference(item));
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
        AddToTrailerDictionary(KnownNames.Size, new PdfInteger(nextObject));
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

    private class ObjectStreamContextImpl : IDisposable
    {
        private readonly LowLevelDocumentBuilder parent;
        private readonly DictionaryBuilder dictionaryBuilder;

        public ObjectStreamContextImpl(LowLevelDocumentBuilder parent, DictionaryBuilder dictionaryBuilder)
        {
            this.parent = parent;
            this.dictionaryBuilder = dictionaryBuilder;
        }

        public void Dispose()
        {
            var capturedBuilder = parent.objectStreamBuilder ??
                      throw new InvalidOperationException("No parent object stream builder");
            parent.objectStreamBuilder = null;
            if (capturedBuilder.HasValues())
            {
                parent.AddDelayedObject(()=> capturedBuilder.CreateStream(dictionaryBuilder));
            }
        }
    }
}