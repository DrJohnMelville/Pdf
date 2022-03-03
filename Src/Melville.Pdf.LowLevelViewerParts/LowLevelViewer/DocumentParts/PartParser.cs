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
        var sourceList = OrderedListOfObjects(lowlevel);
        
        var items = sourceList.Length > 1000?
              CreateLazyLoadList(sourceList)
            : await CreateFlatItemsList(waiting, sourceList);

        await AddPrefixAndSuffix(items, lowlevel);
        return items.ToArray();
    }

    private DocumentPart[] CreateLazyLoadList(PdfIndirectReference[] sourceList)
    {
        var listLen = ComputeDecimatedLength(sourceList);
        var ret = new DocumentPart[listLen + 2];
        for (int i = 0; i < listLen; i++)
        {
            ret[i + 1] =
                new ItemLoader(sourceList.AsMemory(i * 1000, 
                    Math.Min((i+1) * 1000, sourceList.Length) - (i * 1000)));
        }
        return ret;
    }

    private static int ComputeDecimatedLength(PdfIndirectReference[] sourceList) => (sourceList.Length + 999) / 1000;

    private static async Task<DocumentPart[]> CreateFlatItemsList(IWaitingService waiting, PdfIndirectReference[] sourceList)
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

    private static PdfIndirectReference[] OrderedListOfObjects(PdfLowLevelDocument lowlevel)
    {
        return lowlevel.Objects.Values.OrderBy(i => i.Target.ObjectNumber).ToArray();
    }

    private DocumentPart GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}");
}