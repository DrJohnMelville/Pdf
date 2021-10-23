using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Microsoft.CodeAnalysis.CSharp;

namespace Melville.Pdf.Model.Creators;

public interface IPageTreeNodeChild
{
    public (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize);
}
public class PageTreeNodeCreator: IPageTreeNodeChild
{
    private readonly Dictionary<PdfName, PdfObject> metaData;
    private readonly IList<IPageTreeNodeChild> children;

    private PageTreeNodeCreator(Dictionary<PdfName, PdfObject> metaData, IList<IPageTreeNodeChild> children)
    {
        this.metaData = metaData;
        this.children = children;
        metaData[KnownNames.Type] = KnownNames.Pages;
    }
    public PageTreeNodeCreator():this(new Dictionary<PdfName, PdfObject>() ,new List<IPageTreeNodeChild>())
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


    public (PdfIndirectReference Reference, int PageCount)
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize) =>
        TrySegmentedPageTree(maxNodeSize).InnnerConstructPageTree(creator, parent, maxNodeSize);

    private PageTreeNodeCreator TrySegmentedPageTree(int maxNodeSize) =>
        children.Count <= maxNodeSize ? this : 
            SegmentedTree(maxNodeSize).TrySegmentedPageTree(maxNodeSize);

    private PageTreeNodeCreator SegmentedTree(int maxNodeSize) => new(metaData, 
        children.Chunk(maxNodeSize)
        .Select(i => (IPageTreeNodeChild)new PageTreeNodeCreator(
            new Dictionary<PdfName, PdfObject>(),i)).ToArray()
        );

    private (PdfIndirectReference Reference, int PageCount)
        InnnerConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize)
    {
        var ret = creator.Add(new PdfDictionary(metaData));
        TryAddParentReference(parent);
        var kids = new PdfObject[children.Count];
        int count = 0;
        for (int i = 0; i < kids.Length; i++)
        {
            (kids[i], var localCount) = children[i].ConstructPageTree(creator, ret, maxNodeSize);
            count += localCount;
        }
        metaData.Add(KnownNames.Kids, new PdfArray(kids));
        metaData.Add(KnownNames.Count, new PdfInteger(count));
        return (ret, count);
    }
    
    private void TryAddParentReference(PdfIndirectReference? parent)
    {
        if (parent is not null)
        {
            metaData.Add(KnownNames.Parent, parent);
        }
    }
}