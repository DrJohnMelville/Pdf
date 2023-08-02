using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// This creates the nodes of the page tree.
/// </summary>
public sealed class PageTreeNodeCreator: ItemWithResourceDictionaryCreator
{
    private readonly IList<ItemWithResourceDictionaryCreator> children;
    private readonly int maxNodeSize;

    private PageTreeNodeCreator(DictionaryBuilder metaData, IList<ItemWithResourceDictionaryCreator> children, int maxNodeSize):
        base(metaData)
    {
        this.children = children;
        this.maxNodeSize = maxNodeSize;
        metaData.WithItem(KnownNames.Type, KnownNames.Pages);
    }

    /// <summary>
    /// Construct a new PageTreeNodeCreator
    /// </summary>
    /// <param name="maxNodeSize">The maximum number of nodes or pages in a page tree node</param>
    public PageTreeNodeCreator(int maxNodeSize):this(new() ,new List<ItemWithResourceDictionaryCreator>(), maxNodeSize)
    {
    }

    /// <summary>
    /// Create a PageCreator that utilizes root level objects
    /// </summary>
    /// <returns>A PageCreator that can be used to define the page.</returns>
    public PageCreator CreatePage() => AddAndReturn(new PageCreator(NoObjectStream.Instance));

    /// <summary>
    /// A page creator that writes all of its objects to an object stream.
    /// </summary>
    /// <returns>The page creator used to define the page.</returns>
    public PageCreator CreatePageInObjectStream() => 
        AddAndReturn(new PageCreator(EncodeInObjectStream.Instance));

    /// <summary>
    /// Create a subnode of this PageTreeNode
    /// </summary>
    /// <returns>The child PageTreeNodeCreator</returns>
    public PageTreeNodeCreator CreateSubnode() => AddAndReturn(new PageTreeNodeCreator(maxNodeSize));

    private T AddAndReturn<T>(T ret) where T:ItemWithResourceDictionaryCreator
    {
        children.Add(ret);
        return ret;
    }


    /// <inheritdoc />
    public override (PdfIndirectObject Reference, int PageCount)
        ConstructItem(IPdfObjectCreatorRegistry creator, PdfIndirectObject parent) =>
        TrySegmentedPageTree().InnnerConstructPageTree(creator, parent);

    private PageTreeNodeCreator TrySegmentedPageTree() =>
        children.Count <= maxNodeSize ? this : 
            SegmentedTree().TrySegmentedPageTree();

    private PageTreeNodeCreator SegmentedTree() => new(MetaData, 
        children.Chunk(maxNodeSize)
        .Select(i => (ItemWithResourceDictionaryCreator)new PageTreeNodeCreator(
            new(),i, maxNodeSize)).ToArray(), maxNodeSize
        );

    private (PdfIndirectObject Reference, int PageCount)
        InnnerConstructPageTree(IPdfObjectCreatorRegistry creator, PdfIndirectObject parent)
    {
        var ret = creator.Add(PdfDirectObject.CreateNull());
        AddExtraFieldsFromTreeLevel(creator,parent);
        var kids = new PdfIndirectObject[children.Count];
        int count = 0;
        for (var i = 0; i < kids.Length; i++)
        {
            (kids[i], var localCount) = children[i].ConstructItem(creator, ret);
            count += localCount;
        }
        MetaData.WithItem(KnownNames.Kids, new PdfArray(kids)).
            WithItem(KnownNames.Count, count);
        creator.Reassign(ret, MetaData.AsDictionary());
        return (ret, count);
    }
    
    private void AddExtraFieldsFromTreeLevel(
        IPdfObjectCreatorRegistry creator, PdfIndirectObject parent)
    {
        if (parent.TryGetEmbeddedDirectValue(out var dirPar) && dirPar.IsNull )
        {
            Resources.Add((KnownNames.ProcSet, KnownNames.ProcSet), cr => cr.Add(DefaultProcSet()));
        }
        else
        {
            MetaData.WithItem(KnownNames.Parent, parent);
        }

        TryAddResources(creator);
    }

    // Per standard section 14.2, the procset is deprecated.  We just add a default procset requesting
    // the entire set of ProcSets for backward compatibility with older readers.
    private static PdfArray DefaultProcSet() => new(
        KnownNames.PDF, KnownNames.Text, KnownNames.ImageB,
        KnownNames.ImageC, KnownNames.ImageI);
}