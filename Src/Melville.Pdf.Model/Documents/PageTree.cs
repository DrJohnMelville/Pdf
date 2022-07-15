using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
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
        (await LowLevel.GetAsync<PdfNumber>(KnownNames.Count).CA()).IntValue;

    public async IAsyncEnumerator<PdfPage> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        var kids = await KidsAsync().CA();
        await foreach (var kid in kids.CA())
        {
            var kidAsDict = (PdfDictionary)kid;
            var type = await kidAsDict.GetAsync<PdfName>(KnownNames.Type).CA();
            if (type == KnownNames.Page) yield return new PdfPage(kidAsDict);
            else if (type == KnownNames.Pages)
            {
                await foreach (var innerKid in new PageTree(kidAsDict).CA())
                {
                    yield return innerKid;
                }
            }
            else throw new PdfParseException("Page tree should only contain pages and nodes");
        }
    }

    public async ValueTask<HasRenderableContentStream> GetPageAsync(long position)
    {
        var items = await KidsAsync().CA();
        foreach (var kidTask in items.RawItems)
        {
            var kid = (PdfDictionary)await kidTask.DirectValueAsync().CA();
            var type = await kid.GetAsync<PdfName>(KnownNames.Type).CA();
            if (type == KnownNames.Page)
            {
                if (position == 0) return new PdfPage(kid);
                position--;
            }
            else if (type == KnownNames.Pages)
            {
                var nodeCount = (await kid.GetAsync<PdfNumber>(KnownNames.Count).CA()).IntValue;
                if (position < nodeCount) return await new PageTree(kid).GetPageAsync(position).CA();
                position -= nodeCount;
            }
            else throw new PdfParseException("Page trees should contain only pages and nodes");
        }
        throw new IndexOutOfRangeException();
    }

    public ValueTask<PdfArray> KidsAsync() => LowLevel.GetAsync<PdfArray>(KnownNames.Kids);
}