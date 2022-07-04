using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentBuilder
{
    PdfIndirectObject AsIndirectReference(PdfObject? value = null);
    void AssignValueToReference(PdfIndirectObject reference, PdfObject value);
    PdfIndirectObject Add(PdfObject item);
    public PdfIndirectObject Add(PdfObject item, int objectNumber, int generation);
    void AddToTrailerDictionary(PdfName key, PdfObject item);
    public PdfArray EnsureDocumentHasId();
    public byte[] UserPassword { get; set; }
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
    private readonly Dictionary<PdfName, PdfObject> trailerDictionaryItems = new();
    private ObjectStreamBuilder? objectStreamBuilder;
        
    public byte[] UserPassword { get; set; } = Array.Empty<byte>();

    public LowLevelDocumentBuilder(int nextObject = 1)
    {
        this.nextObject = nextObject;
    }
        
    public PdfIndirectObject AsIndirectReference(PdfObject? value = null) =>
        value switch
        {
            PdfIndirectObject pio => pio,
            _ => new PdfIndirectObject(nextObject++, 0, value ?? PdfTokenValues.Null)
        };

    public void AssignValueToReference(PdfIndirectObject reference, PdfObject value)
    {
        ((IMultableIndirectObject)reference).SetValue(value);
    }
    public PdfIndirectObject Add(PdfObject item) => InnerAdd(AsIndirectReference(item));
    public PdfIndirectObject Add(PdfObject item, int objectNumber, int generation) => 
        InnerAdd(new PdfIndirectObject(objectNumber, generation, item));

    public void AddDelayedObject(Func<ValueTask<PdfObject>> creator)
    {
        var reference = new PdfIndirectObject(nextObject++, 0, creator);
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
        trailerDictionaryItems[key] = item;
        
    public PdfDictionary CreateTrailerDictionary()
    {
        AddLengthToTrailerDictionary();
        return new(new Dictionary<PdfName, PdfObject>(trailerDictionaryItems));
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
            var obs = parent.objectStreamBuilder ??
                      throw new InvalidOperationException("No parent object stream builder");
            parent.objectStreamBuilder = null;
            parent.AddDelayedObject(()=> obs.CreateStream(dictionaryBuilder));
        }
    }
}