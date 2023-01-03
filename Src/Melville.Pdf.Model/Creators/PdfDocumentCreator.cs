using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class PdfDocumentCreator
{
    public ILowLevelDocumentCreator LowLevelCreator { get; } = LowLevelDocumentBuilderFactory.New();
    private readonly DictionaryBuilder rootItems = new();
    public PageTreeNodeCreator Pages { get; } = new();
    public int MaxPageTreeNodeSize { get; set; } = 100;

    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7)
    {
        var pageTree = CreateResourceDictionaryItem(Pages);
        rootItems.WithItem(KnownNames.Pages, pageTree);
        LowLevelCreator.AddRootElement(rootItems.AsDictionary());
        return LowLevelCreator.CreateDocument(major, minor);
    }

    public PdfIndirectObject CreateResourceDictionaryItem(ItemWithResourceDictionaryCreator creator) => 
        creator.ConstructPageTree(LowLevelCreator, null, MaxPageTreeNodeSize).Reference;

    public void SetVersionInCatalog(byte major, byte minor) =>
        SetVersionInCatalog(NameDirectory.Get($"{major}.{minor}"));
    public void SetVersionInCatalog(PdfName version) => rootItems.WithItem(KnownNames.Version, version);

    public void AddToRootDictionary(PdfName name, PdfObject obj) => rootItems.WithItem(name, obj);
}
