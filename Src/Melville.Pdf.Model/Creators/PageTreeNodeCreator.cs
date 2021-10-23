using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class PageTreeNodeCreator: PageTreeNodeChildCreator
{
    private readonly IList<PageTreeNodeChildCreator> children;

    private PageTreeNodeCreator(Dictionary<PdfName, PdfObject> metaData, IList<PageTreeNodeChildCreator> children):
        base(metaData)
    {
        this.children = children;
        metaData[KnownNames.Type] = KnownNames.Pages;
    }
    public PageTreeNodeCreator():this(new Dictionary<PdfName, PdfObject>() ,new List<PageTreeNodeChildCreator>())
    {
    }

    public PageCreator CreatePage()
    {
        var ret = new PageCreator();
        children.Add(ret);
        return ret;
    }
    public PageTreeNodeCreator CreateNode()
    {
        var ret = new PageTreeNodeCreator();
        children.Add(ret);
        return ret;
    }


    public override (PdfIndirectReference Reference, int PageCount)
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize) =>
        TrySegmentedPageTree(maxNodeSize).InnnerConstructPageTree(creator, parent, maxNodeSize);

    private PageTreeNodeCreator TrySegmentedPageTree(int maxNodeSize) =>
        children.Count <= maxNodeSize ? this : 
            SegmentedTree(maxNodeSize).TrySegmentedPageTree(maxNodeSize);

    private PageTreeNodeCreator SegmentedTree(int maxNodeSize) => new(MetaData, 
        children.Chunk(maxNodeSize)
        .Select(i => (PageTreeNodeChildCreator)new PageTreeNodeCreator(
            new Dictionary<PdfName, PdfObject>(),i)).ToArray()
        );

    private (PdfIndirectReference Reference, int PageCount)
        InnnerConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize)
    {
        var ret = creator.Add(new PdfDictionary(MetaData));
        AddExtraFieldsFromTreeLevel(creator,parent);
        var kids = new PdfObject[children.Count];
        int count = 0;
        for (int i = 0; i < kids.Length; i++)
        {
            (kids[i], var localCount) = children[i].ConstructPageTree(creator, ret, maxNodeSize);
            count += localCount;
        }
        MetaData.Add(KnownNames.Kids, new PdfArray(kids));
        MetaData.Add(KnownNames.Count, new PdfInteger(count));
        return (ret, count);
    }
    
    private void AddExtraFieldsFromTreeLevel(
        ILowLevelDocumentCreator creator, PdfIndirectReference? parent)
    {
        if (parent is not null)
        {
            MetaData.Add(KnownNames.Parent, parent);
        }
        else
        {
            Resources.Add((KnownNames.ProcSet, KnownNames.ProcSet), DefaultProcSet());
        }

        TryAddResources(creator);
    }

    // Per standard section 14.2, the procset is deprecated.  We just add a default procset requesting
    // the entire set of ProcSets for backward compatibility with older readers.
    private static PdfArray DefaultProcSet() => new(KnownNames.PDF, KnownNames.Text, KnownNames.ImageB,
        KnownNames.ImageC, KnownNames.ImageI);
}