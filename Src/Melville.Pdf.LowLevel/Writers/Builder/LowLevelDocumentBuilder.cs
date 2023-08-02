using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class LowLevelDocumentBuilder : ILowLevelDocumentCreator
{

    [DelegateTo(Visibility = SourceLocationVisibility.Public)] 
    private IPdfObjectCreatorRegistry delegatedItems => registry;
    
    private PdfObjectRegistry registry;
  
    public LowLevelDocumentBuilder(int nextObject = 1)
    {
        registry = new PdfObjectRegistry(nextObject);
    }
        
    
    public PdfArray EnsureDocumentHasId() =>
        registry.TrailerDictionaryItems.TryGetValue(KnownNames.ID, out var val) && 
        val.Value.TryGetEmbeddedDirectValue(out PdfArray ret)
            ? ret
            : AddNewIdArray();

    private PdfArray AddNewIdArray()
    {
        var array = NewIdArray();
        AddToTrailerDictionary(KnownNames.ID, array);
        return array;
    }

    private PdfArray NewIdArray() => new(IdElement(), IdElement());

    private PdfDirectObject IdElement()
    {
        Span<byte> ret = stackalloc byte[32];
        Guid.NewGuid().TryWriteBytes(ret);
        Guid.NewGuid().TryWriteBytes(ret[16..]);
        return PdfDirectObject.CreateString(ret);
    }

    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7) =>
        new PdfLowLevelDocument(major, minor, registry.CreateTrailerDictionary(), 
           new IndirectRegistryWrapper<(int,int )>(registry.Objects));
}
