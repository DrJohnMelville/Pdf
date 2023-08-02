using System.IO;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public interface IPartParser
{
    Task<ParsedLowLevelDocument> ParseAsync(IFile source, IWaitingService waiting);
    public Task<ParsedLowLevelDocument> ParseAsync(Stream source, IWaitingService waiting);
}

public class PartParser: IPartParser
{
    private readonly IPasswordSource passwordSource;

    public PartParser(IPasswordSource passwordSource)
    {
        this.passwordSource = passwordSource;
    }

    public async Task<ParsedLowLevelDocument> ParseAsync(IFile source, IWaitingService waiting) => 
        await ParseAsync(await source.OpenRead(), waiting);

    public async Task<ParsedLowLevelDocument> ParseAsync(Stream source, IWaitingService waiting)
    {
        PdfLowLevelDocument lowlevel = await new PdfLowLevelReader(passwordSource).ReadFromAsync(source);
        return await GenerateUIListAsync(waiting, lowlevel);
    }

    private async Task<ParsedLowLevelDocument> GenerateUIListAsync(IWaitingService waiting, PdfLowLevelDocument lowlevel)
    {
        var sourceList = OrderedListOfObjects(lowlevel).ToArray();
        var items = await new ParsePdfObjectsToView(waiting, sourceList).ParseItemElementsAsync();
        AddPrefixAndSuffix(items, lowlevel);
        return new ParsedLowLevelDocument(items, 
            await CreatePageLookupAsync(lowlevel));
    }

    private static async Task<IPageLookup> CreatePageLookupAsync(PdfLowLevelDocument lowlevel)
    {
        try
        {
            return new PageLookup(await new PdfDocument(lowlevel).PagesAsync());
        }
        catch (Exception )
        {
            return NoPageLookup.Instance;
        }
    }

    private void AddPrefixAndSuffix(DocumentPart[] items, PdfLowLevelDocument lowlevel)
    {
        items[0] = GenerateHeaderElement(lowlevel);
        items[^1] = GenerateSuffixElement(lowlevel);
    }

    private static DocumentPart GenerateSuffixElement(PdfLowLevelDocument lowlevel) => 
        new ViewModelVisitor().GeneratePartAsync("Trailer: ", lowlevel.TrailerDictionary);

    private static IEnumerable<KeyValuePair<(int ObjectNumber, int GenerationNumber), PdfIndirectValue>>
    OrderedListOfObjects(PdfLowLevelDocument lowlevel) => 
        lowlevel.Objects.OrderBy(i => i.Key.ObjectNumber).ThenBy(i=>i.Key.GenerationNumber);

    private DocumentPart GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}");
}