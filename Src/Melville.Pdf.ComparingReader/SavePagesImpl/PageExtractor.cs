using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.LowLevel.Writers.PageExtraction;

namespace Melville.Pdf.ComparingReader.SavePagesImpl;

public readonly partial struct PageExtractor
{
    [FromConstructor] private readonly PdfPage page;
    [FromConstructor] private readonly string contentData;
    private readonly PdfDocumentCreator documentCreator = new();

    public async Task WriteAsync(Stream output)
    {
        var copier = new DeepCopy(documentCreator.LowLevelCreator);
        var targetPage = documentCreator.Pages.CreatePage();
        var targetPromise = targetPage.InitializePromiseObject(documentCreator.LowLevelCreator);

        var (objNum, gen) = await FindRefToPageAsync();
        copier.ReserveIndirectMapping(objNum, gen, targetPromise);
        foreach (var item in page.LowLevel.RawItems)
        {
            if (item.Key.Equals(KnownNames.Contents) || item.Key.Equals(KnownNames.Parent)) continue;
            targetPage.AddMetadata(item.Key,
                await copier.CloneAsync(item.Value));
        }

        targetPage.AddToContentStream(new DictionaryBuilder(), contentData);
        var doc = documentCreator.CreateDocument();
        await new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(output), doc).WriteAsync();
    }

    private async ValueTask<(int objNum, int gen)> FindRefToPageAsync()
    {
        var treeLeaf = (await page.LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.Parent)) ??
                        throw new InvalidDataException("Page does not have a parent");
        var kids = await treeLeaf.GetAsync<PdfArray>(KnownNames.Kids);

        foreach (var value in kids.RawItems)
        {
            var referredObject = await value.LoadValueAsync();
            if (referredObject.TryGet(out PdfDictionary? dict) && dict == page.LowLevel)
                return value.GetObjectReference();
        }

        throw new InvalidDataException("Page should be a child of its parent");
    }
}