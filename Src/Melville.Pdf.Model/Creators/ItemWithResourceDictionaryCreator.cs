using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// Parent of PageCreator and TilePatternCreator, this class generates items that can have
/// a resource dictionary.
/// </summary>
public abstract class ItemWithResourceDictionaryCreator
{
    /// <summary>
    /// Dictionary Builder that will eventually build the target item.  This allows items
    /// to be added to the page or pattern dictionary.
    /// </summary>
    protected DictionaryBuilder MetaData { get; }
    /// <summary>
    /// A dictionary of resource items, indexed by a resource type/name key.  The delegate
    /// creates the given content from an IPdfObjectRegistry.
    /// </summary>
    protected Dictionary<(PdfDirectObject DictionaryName, PdfDirectObject ItemName), 
            Func<IPdfObjectCreatorRegistry,PdfIndirectObject>> Resources { get; } = new();

    /// <summary>
    /// Create the ItemsWithResourceDictionaryCreator
    /// </summary>
    /// <param name="metaData"></param>
    protected ItemWithResourceDictionaryCreator(DictionaryBuilder metaData)
    {
        MetaData = metaData;
    }

    /// <summary>
    /// Construct the item that is crated by this creator
    /// </summary>
    /// <param name="creator">IPdfObjectRegistry that shold be used to create new top level objects</param>
    /// <param name="parent">The parent of this item.</param>
    /// <returns>A reference to the object created and the number of pages created by the method call.</returns>
    public abstract (PdfIndirectObject Reference, int PageCount)
        ConstructItem(IPdfObjectCreatorRegistry creator, PdfIndirectObject parent);

    /// <summary>
    /// Add an item to the top level item dictionary
    /// </summary>
    /// <param name="name">The key of the item to add.</param>
    /// <param name="item">The item to add</param>
    public void AddMetadata(PdfDirectObject name, PdfIndirectObject item) =>
        MetaData.WithItem(name, item);

    private protected void TryAddResources(IPdfObjectCreatorRegistry creator)
    {
        if (Resources.Count == 0) return;
        var res = new DictionaryBuilder();
        foreach (var subDictionary in Resources.GroupBy(i=>i.Key.DictionaryName))
        {
            res.WithItem(subDictionary.Key, DictionaryValues(creator, subDictionary));
        }
        MetaData.WithItem(KnownNames.ResourcesTName, res.AsDictionary());
    }

    private PdfIndirectObject DictionaryValues(
        IPdfObjectCreatorRegistry creator, 
        IGrouping<PdfDirectObject, KeyValuePair<(PdfDirectObject DictionaryName, PdfDirectObject ItemName), 
            Func<IPdfObjectCreatorRegistry,PdfIndirectObject>>> subDictionary) =>
        subDictionary.Key.Equals(KnownNames.ProcSetTName)
            ? subDictionary.First().Value(creator)
            : CreateDictionary(subDictionary, creator);

    private PdfDictionary CreateDictionary(
        IEnumerable<KeyValuePair<(PdfDirectObject DictionaryName, PdfDirectObject ItemName), 
            Func<IPdfObjectCreatorRegistry, PdfIndirectObject>>> items,
        IPdfObjectCreatorRegistry creator) => items
            .Aggregate(new DictionaryBuilder(),
                (builder, item) => builder.WithItem(item.Key.ItemName, creator.AddIfDirect(item.Value(creator))))
            .AsDictionary();

    /// <summary>
    /// Add an object to the resource dictionary
    /// </summary>
    /// <param name="resourceType">Type of object</param>
    /// <param name="name">Key for the object</param>
    /// <param name="obj">The object to add</param>
    public void AddResourceObject(ResourceTypeName resourceType, PdfDirectObject name, PdfIndirectObject obj) =>
        AddResourceObject(resourceType, name, _ => obj);

    /// <summary>
    /// Add an object to the resource dictionary
    /// </summary>
    /// <param name="resourceType">Type of object</param>
    /// <param name="name">Key for the object</param>
    /// <param name="obj">A delegate that will create the object from a IPdfObjectRegistry</param>
    public void AddResourceObject(
        ResourceTypeName resourceType, PdfDirectObject name, Func<IPdfObjectCreatorRegistry,PdfIndirectObject> obj)
    {
        if (!name.IsName)
            throw new InvalidOperationException("Resource key must be a name");
        Resources[(resourceType, name)] = obj;
    }

    /// <summary>
    /// Add a box to the item metadata.  Pages and Pagetrees can have box objects.
    /// </summary>
    /// <param name="name">The type of box to add</param>
    /// <param name="rect">The boc data.</param>
    public void AddBox(BoxName name, in PdfRect rect) => MetaData.WithItem(name, rect.ToPdfArray);

    /// <summary>
    /// Add a rotate declaration to the page dictionary
    /// </summary>
    /// <param name="rotation">The desired rotation value.</param>
    public void AddRotate(int rotation) => MetaData.WithItem(KnownNames.RotateTName, rotation);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PdfDirectObject of the font</returns>
    public PdfDirectObject AddStandardFont(
        ReadOnlySpan<byte> assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(PdfDirectObject.CreateName(assignedName), baseFont, encoding);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PdfDirectObject of the font</returns>
    public PdfDirectObject AddStandardFont(
        PdfDirectObject assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(assignedName, baseFont, (PdfDirectObject)encoding);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PdfDirectObject of the font</returns>
    public PdfDirectObject AddStandardFont(
        PdfDirectObject assignedName, BuiltInFontName baseFont, PdfIndirectObject encoding)
    {
        AddResourceObject(ResourceTypeName.Font, assignedName,
            new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.NameTName, assignedName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectObject)baseFont)
            .WithItem(KnownNames.EncodingTName, encoding)
            .AsDictionary());
        return assignedName;
    }
}