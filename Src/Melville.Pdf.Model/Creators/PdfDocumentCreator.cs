using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class PdfDocumentCreator
{
    public ILowLevelDocumentCreator LowLevelCreator { get; } = new LowLevelDocumentCreator();
    private readonly Dictionary<PdfName, PdfObject> catalogItems = new();
    public PageTreeNodeCreator Pages { get; } = new();
    public int MaxPageTreeNodeSize { get; set; } = 100;

    public PdfLowLevelDocument CreateDocument()
    {
        var (pageTree, _) = Pages.ConstructPageTree(LowLevelCreator, null, MaxPageTreeNodeSize);
        catalogItems.Add(KnownNames.Pages, pageTree);
        LowLevelCreator.AddToTrailerDictionary(KnownNames.Root, LowLevelCreator.Add(
            new PdfDictionary(catalogItems)));
        
        return LowLevelCreator.CreateDocument();
    }

    public void SetVersionInCatalog(byte major, byte minor) =>
        SetVersionInCatalog(NameDirectory.Get($"{major}.{minor}"));
    public void SetVersionInCatalog(PdfName version)
    {
        catalogItems[KnownNames.Version] = version;
    }
}
