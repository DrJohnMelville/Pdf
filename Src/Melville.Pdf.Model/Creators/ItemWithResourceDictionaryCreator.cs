using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
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
    protected Dictionary<(PdfName DictionaryName, PdfName ItemName), 
            Func<IPdfObjectRegistry,PdfObject>> Resources { get; } = new();

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
        ConstructItem(IPdfObjectRegistry creator, PdfIndirectObject? parent);

    /// <summary>
    /// Add an item to the top level item dictionary
    /// </summary>
    /// <param name="name">The key of the item to add.</param>
    /// <param name="item">The item to add</param>
    public void AddMetadata(PdfName name, PdfObject item) =>
        MetaData.WithItem(name, item);

    protected void TryAddResources(IPdfObjectRegistry creator)
    {
        if (Resources.Count == 0) return;
        var res = new DictionaryBuilder();
        foreach (var subDictionary in Resources.GroupBy(i=>i.Key.DictionaryName))
        {
            res.WithItem(subDictionary.Key, DictionaryValues(creator, subDictionary));
        }
        MetaData.WithItem(KnownNames.Resources, res.AsDictionary());
    }

    private PdfObject DictionaryValues(
        IPdfObjectRegistry creator, 
        IGrouping<PdfName, KeyValuePair<(PdfName DictionaryName, PdfName ItemName), 
            Func<IPdfObjectRegistry,PdfObject>>> subDictionary) =>
        subDictionary.Key == KnownNames.ProcSet
            ? subDictionary.First().Value(creator)
            : CreateDictionary(subDictionary, creator);

    private PdfDictionary CreateDictionary(
        IEnumerable<KeyValuePair<(PdfName DictionaryName, PdfName ItemName), 
            Func<IPdfObjectRegistry, PdfObject>>> items,
        IPdfObjectRegistry creator) => items
            .Aggregate(new DictionaryBuilder(),
                (builder, item) => builder.WithItem(item.Key.ItemName, creator.Add(item.Value(creator))))
            .AsDictionary();

    /// <summary>
    /// Add an object to the resource dictionary
    /// </summary>
    /// <param name="resourceType">Type of object</param>
    /// <param name="name">Key for the object</param>
    /// <param name="obj">The object to add</param>
    public void AddResourceObject(ResourceTypeName resourceType, PdfName name, PdfObject obj) =>
        AddResourceObject(resourceType, name, _ => obj);
    public void AddResourceObject(
        ResourceTypeName resourceType, PdfName name, Func<IPdfObjectRegistry,PdfObject> obj) =>
        Resources[(resourceType, name)] = obj;

    /// <summary>
    /// Add a box to the item metadata.  Pages and Pagetrees can have box objects.
    /// </summary>
    /// <param name="name">The type of box to add</param>
    /// <param name="rect">The boc data.</param>
    public void AddBox(BoxName name, in PdfRect rect) => MetaData.WithItem(name, rect.ToPdfArray);

    public void AddRotate(int rotation) => MetaData.WithItem(KnownNames.Rotate, rotation);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PDFName of the font</returns>
    public PdfName AddStandardFont(
        string assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(NameDirectory.Get(assignedName), baseFont, encoding);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PDFName of the font</returns>
    public PdfName AddStandardFont(
        PdfName assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(assignedName, baseFont, (PdfName)encoding);

    /// <summary>
    /// Add a standard font reference to the resource divtionary
    /// </summary>
    /// <param name="assignedName">The name assigned to the font.</param>
    /// <param name="baseFont">The base font for the font</param>
    /// <param name="encoding">The desired encoding</param>
    /// <returns>The PDFName of the font</returns>
    public PdfName AddStandardFont(
        PdfName assignedName, BuiltInFontName baseFont, PdfObject encoding)
    {
        Resources[(KnownNames.Font, assignedName)] = _=>new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.Name, assignedName)
            .WithItem(KnownNames.BaseFont, baseFont)
            .WithItem(KnownNames.Encoding, encoding)
            .AsDictionary();
        return assignedName;
    }
}