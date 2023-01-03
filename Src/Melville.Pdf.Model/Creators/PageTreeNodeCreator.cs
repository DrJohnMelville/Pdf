using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public sealed class PageTreeNodeCreator: ItemWithResourceDictionaryCreator
{
    private readonly IList<ItemWithResourceDictionaryCreator> children;

    private PageTreeNodeCreator(DictionaryBuilder metaData, IList<ItemWithResourceDictionaryCreator> children):
        base(metaData)
    {
        this.children = children;
        metaData.WithItem(KnownNames.Type, KnownNames.Pages);
    }
    public PageTreeNodeCreator():this(new() ,new List<ItemWithResourceDictionaryCreator>())
    {
    }

    public PageCreator CreatePage() => AddAndReturn(new PageCreator(NoObjectStream.Instance));
    public PageCreator CreatePageInObjectStream() => 
        AddAndReturn(new PageCreator(EncodeInObjectStream.Instance));
    public PageTreeNodeCreator CreateNode() => AddAndReturn(new PageTreeNodeCreator());

    private T AddAndReturn<T>(T ret) where T:ItemWithResourceDictionaryCreator
    {
        children.Add(ret);
        return ret;
    }



    public override (PdfIndirectObject Reference, int PageCount)
        ConstructPageTree(IPdfObjectRegistry creator, PdfIndirectObject? parent,
            int maxNodeSize) =>
        TrySegmentedPageTree(maxNodeSize).InnnerConstructPageTree(creator, parent, maxNodeSize);

    private PageTreeNodeCreator TrySegmentedPageTree(int maxNodeSize) =>
        children.Count <= maxNodeSize ? this : 
            SegmentedTree(maxNodeSize).TrySegmentedPageTree(maxNodeSize);

    private PageTreeNodeCreator SegmentedTree(int maxNodeSize) => new(MetaData, 
        children.Chunk(maxNodeSize)
        .Select(i => (ItemWithResourceDictionaryCreator)new PageTreeNodeCreator(
            new(),i)).ToArray()
        );

    private (PdfIndirectObject Reference, int PageCount)
        InnnerConstructPageTree(IPdfObjectRegistry creator, PdfIndirectObject? parent,
            int maxNodeSize)
    {
        var ret = creator.AddPromisedObject();
        AddExtraFieldsFromTreeLevel(creator,parent);
        var kids = new PdfObject[children.Count];
        int count = 0;
        for (int i = 0; i < kids.Length; i++)
        {
            (kids[i], var localCount) = children[i].ConstructPageTree(creator, ret, maxNodeSize);
            count += localCount;
        }
        MetaData.WithItem(KnownNames.Kids, new PdfArray(kids)).
            WithItem(KnownNames.Count, count);
        ret.SetValue(MetaData.AsDictionary());
        return (ret, count);
    }
    
    private void AddExtraFieldsFromTreeLevel(
        IPdfObjectRegistry creator, PdfIndirectObject? parent)
    {
        if (parent is not null)
        {
            MetaData.WithItem(KnownNames.Parent, parent);
        }
        else
        {
            Resources.Add((KnownNames.ProcSet, KnownNames.ProcSet), cr=>cr.Add(DefaultProcSet()));
        }
        TryAddResources(creator);
    }

    // Per standard section 14.2, the procset is deprecated.  We just add a default procset requesting
    // the entire set of ProcSets for backward compatibility with older readers.
    private static PdfArray DefaultProcSet() => new(KnownNames.PDF, KnownNames.Text, KnownNames.ImageB,
        KnownNames.ImageC, KnownNames.ImageI);
}