using System.IO;
using System.Runtime.Intrinsics.Arm;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public interface IPartParser
{
    Task<DocumentPart[]> ParseAsync(IFile source, IWaitingService waiting);
    public Task<DocumentPart[]> ParseAsync(Stream source, IWaitingService waiting);
}

public class PartParser: IPartParser
{
    private readonly IPasswordSource passwordSource;

    public PartParser(IPasswordSource passwordSource)
    {
        this.passwordSource = passwordSource;
    }

    public async Task<DocumentPart[]> ParseAsync(IFile source, IWaitingService waiting) => 
        await ParseAsync(await source.OpenRead(), waiting);

    public async Task<DocumentPart[]> ParseAsync(Stream source, IWaitingService waiting)
    {
        PdfLowLevelDocument lowlevel = await RandomAccessFileParser.Parse(
            new ParsingFileOwner(source, passwordSource));
        return await GenerateUIList(waiting, lowlevel);
    }

    private async Task<DocumentPart[]> GenerateUIList(IWaitingService waiting, PdfLowLevelDocument lowlevel)
    {
        var sourceList = OrderedListOfObjects(lowlevel);
        var items = await ParseItemElements(waiting, sourceList);
        await AddPrefixAndSuffix(items, lowlevel);
        return items;
    }

    private const int maxSegmentLength = 1000;
    private async Task<DocumentPart[]> ParseItemElements(IWaitingService waiting, PdfIndirectObject[] sourceList)
    {
        return TooLongForPreloadedList(sourceList)?
            CreateLazyLoadList(sourceList)
            : await CreatePreloadedList(waiting, sourceList);
    }

    private static bool TooLongForPreloadedList(PdfIndirectObject[] sourceList) => 
        sourceList.Length > maxSegmentLength;

    private DocumentPart[] CreateLazyLoadList(PdfIndirectObject[] sourceList)
    {
        var listLen = ComputeDecimatedLength(sourceList);
        var ret = new DocumentPart[listLen + 2];
        for (int i = 0; i < listLen; i++)
        {
            ret[i + 1] =
                new ItemLoader(MemoryForSegment(sourceList, i));
        }
        return ret;
    }

    private static Memory<PdfIndirectObject> MemoryForSegment(PdfIndirectObject[] sourceList, int i) => 
        sourceList.AsMemory(i * maxSegmentLength, SegmentLength(sourceList, i));

    private static int SegmentLength(PdfIndirectObject[] sourceList, int i) => 
        ItemAfterSegment(sourceList, i) - (i * maxSegmentLength);

    private static int ItemAfterSegment(PdfIndirectObject[] sourceList, int i)
    {
        return Math.Min((i+1) * maxSegmentLength, sourceList.Length);
    }

    private static int ComputeDecimatedLength(PdfIndirectObject[] sourceList) => 
        (sourceList.Length + maxSegmentLength - 1) / maxSegmentLength;

    private static async Task<DocumentPart[]> CreatePreloadedList(IWaitingService waiting, PdfIndirectObject[] sourceList)
    {
        var items = new DocumentPart[sourceList.Length + 2];
        var creator = new ItemLoader(sourceList);
        await creator.FillMemoryWithParts(waiting, items.AsMemory(1..^1));
        return items;
    }

    private async ValueTask AddPrefixAndSuffix(DocumentPart[] items, PdfLowLevelDocument lowlevel)
    {
        items[0] = GenerateHeaderElement(lowlevel);
        items[^1] = await GenerateSuffixElement(lowlevel);
    }

    private static ValueTask<DocumentPart> GenerateSuffixElement(PdfLowLevelDocument lowlevel)
    {
        return new ViewModelVisitor().GeneratePart("Trailer: ", lowlevel.TrailerDictionary);
    }

    private static PdfIndirectObject[] OrderedListOfObjects(PdfLowLevelDocument lowlevel)
    {
        return lowlevel.Objects.Values.OrderBy(i => i.ObjectNumber).ToArray();
    }

    private DocumentPart GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}");
}