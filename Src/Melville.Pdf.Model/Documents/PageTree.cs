using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Documents;

public readonly struct PageTree: IAsyncEnumerable<PdfPage>
{
    public PdfDictionary LowLevel { get; }

    public PageTree(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public async ValueTask<long> CountAsync() => 
        (await LowLevel.GetAsync<PdfNumber>(KnownNames.Count).ConfigureAwait(false)).IntValue;

    public async IAsyncEnumerator<PdfPage> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        var kids = await LowLevel.GetAsync<PdfArray>(KnownNames.Kids).ConfigureAwait(false);
        await foreach (var kid in kids.ConfigureAwait(false))
        {
            var kidAsDict = (PdfDictionary)kid;
            var type = await kidAsDict.GetAsync<PdfName>(KnownNames.Type).ConfigureAwait(false);
            if (type == KnownNames.Page) yield return new PdfPage(kidAsDict);
            else if (type == KnownNames.Pages)
            {
                await foreach (var innerKid in new PageTree(kidAsDict).ConfigureAwait(false))
                {
                    yield return innerKid;
                }
            }
            else throw new PdfParseException("Page tree should only contain pages and nodes");
        }
    }

    public async Task<PdfPage> GetPageAsync(long position)
    {
        var items = await LowLevel.GetAsync<PdfArray>(KnownNames.Kids).ConfigureAwait(false);
        foreach (var kidTask in items.RawItems)
        {
            var kid = (PdfDictionary)await kidTask.DirectValueAsync().ConfigureAwait(false);
            var type = await kid.GetAsync<PdfName>(KnownNames.Type).ConfigureAwait(false);
            if (type == KnownNames.Page)
            {
                if (position == 0) return new PdfPage(kid);
                position--;
            }
            else if (type == KnownNames.Pages)
            {
                var nodeCount = (await kid.GetAsync<PdfNumber>(KnownNames.Count).ConfigureAwait(false)).IntValue;
                if (position < nodeCount) return await new PageTree(kid).GetPageAsync(position).ConfigureAwait(false);
                position -= nodeCount;
            }
            else throw new PdfParseException("Page trees should contain only pages and nodes");
        }

        throw new IndexOutOfRangeException();
    }
}