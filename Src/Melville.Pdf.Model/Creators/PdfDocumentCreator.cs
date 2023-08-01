using System.Collections.Generic;
using System.Linq.Expressions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// Used to create a PdfLowLevelDocument by calling methods on a collection of creator classes.
/// </summary>
public class PdfDocumentCreator
{
    /// <summary>
    /// The LowLevelDocumentCreator that will receive the created document
    /// </summary>
    public ILowLevelDocumentCreator LowLevelCreator { get; } = LowLevelDocumentBuilderFactory.New();
    private readonly ValueDictionaryBuilder rootItems = new();
    /// <summary>
    /// The pages that will be included in the new document.
    /// </summary>
    public PageTreeNodeCreator Pages { get; }

    /// <summary>
    /// Create a PdfDocumentCreator
    /// </summary>
    /// <param name="maxPageTreeNodeSize">Maximum pages or subnodes in a page tree node</param>
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
        rootItems.WithItem(KnownNames.PagesTName, pageTree);
        LowLevelCreator.AddRootElement(rootItems.AsDictionary());
        return LowLevelCreator.CreateDocument(major, minor);
    }

    private PdfIndirectValue CreateResourceDictionaryItem(ItemWithResourceDictionaryCreator creator) => 
        creator.ConstructItem(LowLevelCreator, PdfDirectValue.CreateNull()).Reference;

    /// <summary>
    /// Set a version number in the catalog, which may be different from the version number in the header.
    /// </summary>
    /// <param name="major">Major version number</param>
    /// <param name="minor">Minor version number</param>
    public void SetVersionInCatalog(byte major, byte minor) =>
        SetVersionInCatalog(PdfDirectValue.CreateName($"{major}.{minor}"));

    /// <summary>
    /// Set a version number in the catalog, which may be different from the version number in the header.
    /// </summary>
    /// <param name="version">Version number as a PdfName</param>
    public void SetVersionInCatalog(PdfDirectValue version) => rootItems.WithItem(KnownNames.VersionTName, version);

    /// <summary>
    /// Add an item to the document's root dictionaru
    /// </summary>
    /// <param name="name">Key for the added item</param>
    /// <param name="obj">The object to be added.</param>
    public void AddToRootDictionary(PdfDirectValue name, PdfIndirectValue obj) => rootItems.WithItem(name, obj);
}
