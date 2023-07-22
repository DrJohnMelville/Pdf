using System.IO;
using System.Runtime.Intrinsics.Arm;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
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
        var sourceList = OrderedListOfObjects(lowlevel);
        var items = await new ParsePdfObjectsToView(waiting, sourceList).ParseItemElementsAsync();
        await AddPrefixAndSuffixAsync(items, lowlevel);
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

    private async ValueTask AddPrefixAndSuffixAsync(DocumentPart[] items, PdfLowLevelDocument lowlevel)
    {
        items[0] = GenerateHeaderElement(lowlevel);
        items[^1] = await GenerateSuffixElementAsync(lowlevel);
    }

    private static ValueTask<DocumentPart> GenerateSuffixElementAsync(PdfLowLevelDocument lowlevel) => 
        new ViewModelVisitor().GeneratePartAsync("Trailer: ", lowlevel.TrailerDictionary);

    private static PdfIndirectValue[] OrderedListOfObjects(PdfLowLevelDocument lowlevel) => 
        lowlevel.Objects.Values.OrderBy(i => i.Memento.UInt64s[0]).ToArray();

    private DocumentPart GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}");
}