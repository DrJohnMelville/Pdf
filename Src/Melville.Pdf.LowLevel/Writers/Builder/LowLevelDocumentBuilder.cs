using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class LowLevelDocumentBuilder : ILowLevelDocumentCreator
{

    [DelegateTo(Visibility = SourceLocationVisibility.Public)] 
    private IPdfObjectRegistry delegatedItems => registry;
    
    private PdfObjectRegistry registry;
  
    public LowLevelDocumentBuilder(int nextObject = 1)
    {
        registry = new PdfObjectRegistry(nextObject);
    }
        
    
    public PdfArray EnsureDocumentHasId() =>
        registry.TrailerDictionaryItems.TryGetValue(KnownNames.ID, out var val) && val is PdfArray ret
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

    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7) =>
        new PdfLowLevelDocument(major, minor, registry.CreateTrailerDictionary(), 
            registry.Objects.ToDictionary(
                static item => (item.ObjectNumber, item.GenerationNumber) ));
}