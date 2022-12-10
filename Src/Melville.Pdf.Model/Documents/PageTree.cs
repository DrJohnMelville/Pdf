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

    public async ValueTask<HasRenderableContentStream> GetPageAsync(long pageNumberOneBased)
    {
        List<PdfObject> priorNodes = new();
        var items = await KidsAsync().CA();
        // this is an unrolled recursive function so I 
        for (int i = 0; i < items.RawItems.Count; i++)
        {
            var kid = (PdfDictionary)await items.RawItems[i].DirectValueAsync().CA();
            var type = (await kid.GetAsync<PdfName>(KnownNames.Type).CA()).GetHashCode();
            switch (type)
            {
                case KnownNameKeys.Page:
                    if (IsDesiredPage(pageNumberOneBased)) return new PdfPage(kid);
                    pageNumberOneBased--;
                    break;
                case KnownNameKeys.Pages:
                    if (priorNodes.Contains(kid))
                        throw new PdfParseException("Cycle in Page Tree");
                    priorNodes.Add(kid);

                    var subNode = new PageTree(kid);
                    var nodeCount = await subNode.CountAsync().CA();
                    if (pageNumberOneBased <= nodeCount)
                    {
                        items = await subNode.KidsAsync().CA();
                        i = -1;
                        break;
                    }

                    pageNumberOneBased -= nodeCount;
                    break;
                default:
                    throw new PdfParseException("Page trees should contain only pages and nodes");
            }
        }
        throw new IndexOutOfRangeException();
    }

    private static bool IsDesiredPage(long position)
    {
        // Notice that asking for page 0 or page 1 both produce the first page.  This is intenntional, it
        // allows us to use natural page numbering for documents, which is one based, but allows
        //  a lot of the tests, which render page 0 to work correctly.
        return position <= 1;
    }

    public ValueTask<PdfArray> KidsAsync() => LowLevel.GetAsync<PdfArray>(KnownNames.Kids);
}