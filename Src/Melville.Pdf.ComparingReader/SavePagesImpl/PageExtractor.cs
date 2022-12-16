using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Windows.Globalization;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using DeepCopy = Melville.Pdf.LowLevel.Writers.PageExtraction.DeepCopy;

namespace Melville.Pdf.ComparingReader.SavePagesImpl;

public readonly partial struct PageExtractor
{
    [FromConstructor] private readonly PdfDocument document;
    [FromConstructor] private readonly PdfPage page;
    [FromConstructor] private readonly string contentData;
    private readonly PdfDocumentCreator documentCreator = new();

    public async Task WriteAsync(Stream output)
    {
        var copier = new DeepCopy(documentCreator.LowLevelCreator);
        var targetPage = documentCreator.Pages.CreatePage();
        copier.ReserveIndirectMapping(await FindRefToPage(),
            targetPage.InitializePromiseObject(documentCreator.LowLevelCreator));
        foreach (var item in page.LowLevel.RawItems)
        {
            if (item.Key.GetHashCode() is KnownNameKeys.Contents or KnownNameKeys.Parent) continue;
            targetPage.AddMetadata(item.Key,
                await copier.Clone(item.Value));
        }

        targetPage.AddToContentStream(new DictionaryBuilder(), contentData);
        var doc = documentCreator.CreateDocument();
        await new LowLevelDocumentWriter(PipeWriter.Create(output), doc).WriteWithReferenceStream();
    }

    private async ValueTask<PdfIndirectObject> FindRefToPage()
    {
        var treeLeaf = (await page.GetParentAsync()) ??
                        throw new InvalidDataException("Page does not have a parent");
        var kids = (PdfArray)await treeLeaf.LowLevel[KnownNames.Kids];
        foreach (var value in kids.RawItems)
        {
            if (value is PdfIndirectObject pio && (await pio.DirectValueAsync()) == page.LowLevel)
                return pio;
        }

        throw new InvalidDataException("Page should be a child of its parent");
    }
}