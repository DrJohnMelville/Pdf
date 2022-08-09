using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class PdfDocumentCreator
{
    public ILowLevelDocumentCreator LowLevelCreator { get; } = new LowLevelDocumentCreator();
    private readonly DictionaryBuilder catalogItems = new();
    public PageTreeNodeCreator Pages { get; } = new();
    public int MaxPageTreeNodeSize { get; set; } = 100;

    public PdfLowLevelDocument CreateDocument()
    {
        var pageTree = CreateResourceDictionaryItem(Pages);
        catalogItems.WithItem(KnownNames.Pages, pageTree);
        LowLevelCreator.AddToTrailerDictionary(KnownNames.Root, LowLevelCreator.Add(
            catalogItems.AsDictionary()));
        return LowLevelCreator.CreateDocument();
    }

    public PdfIndirectObject CreateResourceDictionaryItem(ItemWithResourceDictionaryCreator creator) => 
        creator.ConstructPageTree(LowLevelCreator, null, MaxPageTreeNodeSize).Reference;

    public void SetVersionInCatalog(byte major, byte minor) =>
        SetVersionInCatalog(NameDirectory.Get($"{major}.{minor}"));
    public void SetVersionInCatalog(PdfName version) => catalogItems.WithItem(KnownNames.Version, version);

    public void AddToCatalog(PdfName name, PdfObject obj) => catalogItems.WithItem(name, obj);
}
