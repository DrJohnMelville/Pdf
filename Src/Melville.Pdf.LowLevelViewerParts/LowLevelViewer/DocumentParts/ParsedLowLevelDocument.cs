﻿using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public class ParsedLowLevelDocument(DocumentPart[] root, IPageLookup pages, PdfLowLevelDocument? lowlevel): IDisposable
{
    public DocumentPart[] Root { get; } = root;
    public IPageLookup Pages { get; } = pages;
    public void Dispose()
    {
        (lowlevel as IDisposable)?.Dispose();
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
            var kidType = await kid[KnownNames.Type];
            if (kidType.Equals(KnownNames.Page))
            {
                if (page == 0)
                {
                    var ret = kids.RawItems[i];
                    Debug.Assert(!ret.IsEmbeddedDirectValue());
                    return ExtractCrossReference(ret);
                }
                else
                {
                    page--;
                }
            } else if (kidType.Equals(KnownNames.Pages))
            {
                var nodeCount = (int)(await kid.GetAsync<int>(KnownNames.Count));
                if (page < nodeCount) return await InnerPageForNumberAsync(new PageTree(kid), page);
                page -= nodeCount;
            }
        }

        return new CrossReference(0, 0);
    }

    private static CrossReference ExtractCrossReference(PdfIndirectObject ret)
    {
        var retNums = ret.Memento.UInt64s;
        return new CrossReference((int)retNums[0], (int)retNums[1]);
    }
}