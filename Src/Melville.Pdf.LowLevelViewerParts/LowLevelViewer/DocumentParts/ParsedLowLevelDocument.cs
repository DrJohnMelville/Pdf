using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ParsedLowLevelDocument
{
    public DocumentPart[] Root { get; }
    public IPageLookup Pages { get; }

    public ParsedLowLevelDocument(DocumentPart[] root, IPageLookup pages)
    {
        Root = root;
        Pages = pages;
    }
}

public interface IPageLookup
{
    ValueTask<CrossReference> PageForNumberAsync(int page);
}

[StaticSingleton]
public partial class NoPageLookup : IPageLookup
{
    public ValueTask<CrossReference> PageForNumberAsync(int page) => new(new CrossReference(0, 0));
}

public class PageLookup : IPageLookup
{
    private readonly PageTree treeRoot;

    public PageLookup(PageTree treeRoot)
    {
        this.treeRoot = treeRoot;
    }

    public ValueTask<CrossReference> PageForNumberAsync(int page) => 
        InnerPageForNumberAsync(treeRoot, page);

    private async ValueTask<CrossReference> InnerPageForNumberAsync(PageTree node, int page)
    {
        var kids = await node.KidsAsync();
        for (int i = 0; i < kids.Count; i++)
        {
            var kid = await kids.GetAsync<PdfDictionary>(i);
            var kidType = await kid.GetAsync<PdfName>(KnownNames.Type);
            if (kidType == KnownNames.Page)
            {
                if (page == 0)
                {
                    var ret = (PdfIndirectObject)kids.RawItems[i];
                    return new CrossReference(ret.ObjectNumber, ret.GenerationNumber);
                }
                else
                {
                    page--;
                }
            } else if (kidType == KnownNames.Pages)
            {
                var nodeCount = (int)(await kid.GetAsync<PdfNumber>(KnownNames.Count)).IntValue;
                if (page < nodeCount) return await InnerPageForNumberAsync(new PageTree(kid), page);
                page -= nodeCount;
            }
        }

        return new CrossReference(0, 0);
    }
}