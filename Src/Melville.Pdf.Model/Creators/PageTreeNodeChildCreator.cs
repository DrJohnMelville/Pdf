using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public abstract class PageTreeNodeChildCreator
{
    protected Dictionary<PdfName, PdfObject> MetaData { get; }
    protected Dictionary<(PdfName DictionaryName, PdfName ItemName), PdfObject> Resources { get; } = new();

    protected PageTreeNodeChildCreator(Dictionary<PdfName, PdfObject> metaData)
    {
        MetaData = metaData;
    }

    public abstract (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize);

    protected void TryAddResources(ILowLevelDocumentCreator creator)
    {
        if (Resources.Count == 0) return;
        var res = new DictionaryBuilder();
        foreach (var subDictionary in Resources.GroupBy(i=>i.Key.DictionaryName))
        {
            res.WithItem(subDictionary.Key, DictionaryValues(creator, subDictionary));
        }
        MetaData.Add(KnownNames.Resources, res.AsDictionary());
    }

    private PdfObject DictionaryValues(
        ILowLevelDocumentCreator creator, 
        IGrouping<PdfName, KeyValuePair<(PdfName DictionaryName, PdfName ItemName), PdfObject>> subDictionary) =>
        subDictionary.Key == KnownNames.ProcSet
            ? subDictionary.First().Value
            : CreateDictionary(subDictionary, creator);

    private PdfDictionary CreateDictionary(
        IEnumerable<KeyValuePair<(PdfName DictionaryName, PdfName ItemName), PdfObject>> items,
        ILowLevelDocumentCreator creator) => items
            .Aggregate(new DictionaryBuilder(),
                (builder, item) => builder.WithItem(item.Key.ItemName, creator.Add(item.Value)))
            .AsDictionary();

    public void AddResourceObject(ResourceTypeName resourceType, PdfName name, PdfObject obj) => 
        Resources[(resourceType, name)] = obj;

    public void AddBox(BoxName name, in PdfRect rect) => MetaData.Add(name, rect.ToPdfArray);

    public PdfName AddStandardFont(
        string assignedName, BuiltInFontName baseFont, FontEncodingName encoding) =>
        AddStandardFont(NameDirectory.Get(assignedName), baseFont, encoding);
    public PdfName AddStandardFont(
        PdfName assignedName, BuiltInFontName baseFont, FontEncodingName encoding)
    {
        Resources[(KnownNames.Font, assignedName)] = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.Name, assignedName)
            .WithItem(KnownNames.BaseFont, baseFont)
            .WithItem(KnownNames.Encoding, encoding)
            .AsDictionary();
        return assignedName;
    }
}