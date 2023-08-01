using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

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
        
    
    public PdfValueArray EnsureDocumentHasId() =>
        registry.TrailerDictionaryItems.TryGetValue(KnownNames.IDTName, out var val) && 
        val.Value.TryGetEmbeddedDirectValue(out PdfValueArray ret)
            ? ret
            : AddNewIdArray();

    private PdfValueArray AddNewIdArray()
    {
        var array = NewIdArray();
        AddToTrailerDictionary(KnownNames.IDTName, array);
        return array;
    }

    private PdfValueArray NewIdArray() => new(IdElement(), IdElement());

    private PdfDirectValue IdElement()
    {
        Span<byte> ret = stackalloc byte[32];
        Guid.NewGuid().TryWriteBytes(ret);
        Guid.NewGuid().TryWriteBytes(ret[16..]);
        return PdfDirectValue.CreateString(ret);
    }

    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7) =>
        new PdfLowLevelDocument(major, minor, registry.CreateTrailerDictionary(), 
           new IndirectRegistryWrapper<(int,int )>(registry.Objects));
}
