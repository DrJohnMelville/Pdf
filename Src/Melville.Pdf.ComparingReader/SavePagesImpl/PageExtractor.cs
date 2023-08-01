using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
            if (item.Key.Equals(KnownNames.ContentsTName) || item.Key.Equals(KnownNames.ParentTName)) continue;
            targetPage.AddMetadata(item.Key,
                await copier.CloneAsync(item.Value));
        }

        targetPage.AddToContentStream(new ValueDictionaryBuilder(), contentData);
        var doc = documentCreator.CreateDocument();
        await new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(output), doc).WriteAsync();
    }

    private async ValueTask<(int objNum, int gen)> FindRefToPageAsync()
    {
        #warning fix this once we can compile
        // var treeLeaf = (await page.LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.ParentTName)) ??
        //                 throw new InvalidDataException("Page does not have a parent");
        // var kids = (PdfValueArray)await treeLeaf[KnownNames.Kids];
        // foreach (var value in kids.RawItems)
        // {
        //     if (value is PdfIndirectObject pio && (await pio.DirectValueAsync()).Equals(page.LowLevel))
        //         return pio;
        // }

        throw new InvalidDataException("Page should be a child of its parent");
    }
}