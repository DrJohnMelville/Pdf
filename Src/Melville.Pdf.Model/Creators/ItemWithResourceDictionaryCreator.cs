using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public abstract class ItemWithResourceDictionaryCreator
{
    protected DictionaryBuilder MetaData { get; }
    protected Dictionary<(PdfName DictionaryName, PdfName ItemName), 
            Func<ILowLevelDocumentCreator,PdfObject>> Resources { get; } = new();

    protected ItemWithResourceDictionaryCreator(DictionaryBuilder metaData)
    {
        MetaData = metaData;
    }

    public abstract (PdfIndirectObject Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectObject? parent,
            int maxNodeSize);

    protected void TryAddResources(ILowLevelDocumentCreator creator)
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
        ILowLevelDocumentCreator creator, 
        IGrouping<PdfName, KeyValuePair<(PdfName DictionaryName, PdfName ItemName), 
            Func<ILowLevelDocumentCreator,PdfObject>>> subDictionary) =>
        subDictionary.Key == KnownNames.ProcSet
            ? subDictionary.First().Value(creator)
            : CreateDictionary(subDictionary, creator);

    private PdfDictionary CreateDictionary(
        IEnumerable<KeyValuePair<(PdfName DictionaryName, PdfName ItemName), 
            Func<ILowLevelDocumentCreator, PdfObject>>> items,
        ILowLevelDocumentCreator creator) => items
            .Aggregate(new DictionaryBuilder(),
                (builder, item) => builder.WithItem(item.Key.ItemName, creator.Add(item.Value(creator))))
            .AsDictionary();

    public void AddResourceObject(ResourceTypeName resourceType, PdfName name, PdfObject obj) =>
        AddResourceObject(resourceType, name, _ => obj);
    public void AddResourceObject(
        ResourceTypeName resourceType, PdfName name, Func<ILowLevelDocumentCreator,PdfObject> obj) =>
        Resources[(resourceType, name)] = obj;

    public void AddBox(BoxName name, in PdfRect rect) => MetaData.WithItem(name, rect.ToPdfArray);

    public void AddRotate(int rotation) => MetaData.WithItem(KnownNames.Rotate, new PdfInteger(rotation));

    public PdfName AddStandardFont(
        string assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(NameDirectory.Get(assignedName), baseFont, encoding);

    public PdfName AddStandardFont(
        PdfName assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(assignedName, baseFont, (PdfName)encoding);
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