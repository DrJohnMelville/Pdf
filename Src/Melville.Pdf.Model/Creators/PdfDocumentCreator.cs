using System.Collections.Generic;
using System.Linq.Expressions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// Used to create a PdfLowLevelDocument by calling methods on a collection of creator classes.
/// </summary>
public class PdfDocumentCreator
{
    public ILowLevelDocumentCreator LowLevelCreator { get; } = LowLevelDocumentBuilderFactory.New();
    private readonly DictionaryBuilder rootItems = new();
    public PageTreeNodeCreator Pages { get; }

    public PdfDocumentCreator(int maxPageTreeNodeSize = 100)
    {
        Pages = new PageTreeNodeCreator(maxPageTreeNodeSize);
    }

    /// <summary>
    /// Construct the PdfLowLevelDocument
    /// </summary>
    /// <param name="major">The major version number</param>
    /// <param name="minor">The minor version number</param>
    /// <returns>The LowLevelPdfDocument created.</returns>
    public PdfLowLevelDocument CreateDocument(byte major = 1, byte minor = 7)
    {
        var pageTree = CreateResourceDictionaryItem(Pages);
        rootItems.WithItem(KnownNames.Pages, pageTree);
        LowLevelCreator.AddRootElement(rootItems.AsDictionary());
        return LowLevelCreator.CreateDocument(major, minor);
    }

    private PdfIndirectObject CreateResourceDictionaryItem(ItemWithResourceDictionaryCreator creator) => 
        creator.ConstructItem(LowLevelCreator, null).Reference;

    /// <summary>
    /// Set a version number in the catalog, which may be different from the version number in the header.
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    public void SetVersionInCatalog(byte major, byte minor) =>
        SetVersionInCatalog(NameDirectory.Get($"{major}.{minor}"));

    /// <summary>
    /// Set a version number in the catalog, which may be different from the version number in the header.
    /// </summary>
    /// <param name="version">Version number as a PdfName</param>
    public void SetVersionInCatalog(PdfName version) => rootItems.WithItem(KnownNames.Version, version);

    /// <summary>
    /// Add an item to the document's root dictionaru
    /// </summary>
    /// <param name="name">Key for the added item</param>
    /// <param name="obj">The object to be added.</param>
    public void AddToRootDictionary(PdfName name, PdfObject obj) => rootItems.WithItem(name, obj);
}
