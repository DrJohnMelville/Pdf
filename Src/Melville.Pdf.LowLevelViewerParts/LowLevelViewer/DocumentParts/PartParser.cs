using System.IO;
using System.Runtime.Intrinsics.Arm;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
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
        PdfLowLevelDocument lowlevel = await RandomAccessFileParser.Parse(
            new ParsingFileOwner(source, passwordSource));
        return await GenerateUIList(waiting, lowlevel);
    }

    private async Task<ParsedLowLevelDocument> GenerateUIList(IWaitingService waiting, PdfLowLevelDocument lowlevel)
    {
        var sourceList = OrderedListOfObjects(lowlevel);
        var items = await new ParsePdfObjectsToView(waiting, sourceList).ParseItemElements();
        await AddPrefixAndSuffix(items, lowlevel);
        return new ParsedLowLevelDocument(items, 
            await CreatePageLookup(lowlevel));
    }

    private static async Task<IPageLookup> CreatePageLookup(PdfLowLevelDocument lowlevel)
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

    private async ValueTask AddPrefixAndSuffix(DocumentPart[] items, PdfLowLevelDocument lowlevel)
    {
        items[0] = GenerateHeaderElement(lowlevel);
        items[^1] = await GenerateSuffixElement(lowlevel);
    }

    private static ValueTask<DocumentPart> GenerateSuffixElement(PdfLowLevelDocument lowlevel) => 
        new ViewModelVisitor().GeneratePart("Trailer: ", lowlevel.TrailerDictionary);

    private static PdfIndirectObject[] OrderedListOfObjects(PdfLowLevelDocument lowlevel) => 
        lowlevel.Objects.Values.OrderBy(i => i.ObjectNumber).ToArray();

    private DocumentPart GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}");
}