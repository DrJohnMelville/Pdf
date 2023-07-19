using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocuments.LowLevel;
using Moq;
using Xunit;
using DocumentPart = Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.DocumentPart;

namespace Melville.Pdf.WpfToolTests.LowLevelReader;

public class PartParserTest
{
    private readonly Mock<IWaitingService> waitingService = new();
        
    private readonly PartParser sut = new(Mock.Of<IPasswordSource>());
    [Theory]
    [InlineData(1,7)]
    [InlineData(1,3)]
    [InlineData(2,0)]
    public async Task ParseHeaderAsync(byte major, byte minor)
    {
        var model =  await sut.ParseAsync(
            await MinimalPdfParser.MinimalPdf(major, minor).AsFileAsync(),waitingService.Object);
        Assert.Equal($"PDF-{major}.{minor}", model.Root[0].Title);
    }

    [Fact]
    public async Task ParseFinalDictionaryAsync()
    {
        var model = await sut.ParseAsync(
            await MinimalPdfParser.MinimalPdf().AsFileAsync(),
            waitingService.Object);
        var trailerNode = model.Root.Last();
        Assert.Equal("Trailer: Dictionary", trailerNode.Title);
        Assert.Equal(2, trailerNode.Children.Count);
        Assert.Equal("/Size: 5", trailerNode.Children[1].Title);
        Assert.Equal("/Root: 4 0 R", trailerNode.Children[0].Title);
    }

    [Fact]
    public async Task ReportWaitingTimeAsync()
    {
        var model = await sut.ParseAsync(
            await MinimalPdfParser.MinimalPdf().AsFileAsync(),
            waitingService.Object);
        waitingService.Verify(i=>i.WaitBlock("Loading File", 4, false), Times.Once);
        waitingService.Verify(i=>i.MakeProgress(It.IsAny<string?>()), Times.Exactly(4));
        waitingService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ParseArrayAsync()
    {
        var model = await BuildSingleElementFileAsync(_=>
            new PdfArray(PdfBoolean.True, PdfBoolean.False, KnownNames.Max));
        var array = model[1];
        Assert.Equal("1 0: Array", array.Title);
        Assert.Equal("[0]: true", array.Children[0].Title);
        Assert.Equal("[1]: false", array.Children[1].Title);
        Assert.Equal("[2]: /Max", array.Children[2].Title);
    }
    [Fact]
    public async Task ParseSteamAsync()
    {
        var model = await BuildSingleElementFileAsync(i=>
            new DictionaryBuilder().WithItem(KnownNames.Type, KnownNames.C1).AsStream("The Stream Data"));
        var stream = (StreamPartViewModel)model[1];
        Assert.Equal("1 0: Stream", stream.Title);
        Assert.Equal("/Type: /C1", stream.Children[0].Title);
        Assert.Equal("/Length: 15", stream.Children[1].Title);
        stream.SelectedFormat = stream.Formats.Last();
        var str = (ByteStringViewModel)stream.Content!;
        Assert.Equal("00000000  54 68 65 20 53 74 72 65 61 6D 20 44 61 74 61      The Stream Data", str.HexDump);
        Assert.Equal("The Stream Data", str.AsAsciiString);
    }

    private async Task TestSingleElementAsync(PdfObject item, string renderAs)
    {
        var model = await BuildSingleElementFileAsync(_=>item);
        Assert.Equal("1 0: "+renderAs, model[1].Title);
    }

    private async Task<DocumentPart[]> BuildSingleElementFileAsync(
        Func<IPdfObjectCreatorRegistry,PdfObject> item)
    {
        var builder = new LowLevelDocumentBuilder();
        builder.Add(item(builder));
        return (await CreateParsedFileAsync(builder)).Root;
    }

    private async Task<ParsedLowLevelDocument> CreateParsedFileAsync(ILowLevelDocumentCreator builder) => 
        (await sut.ParseAsync(await builder.AsFileAsync(), waitingService.Object));

    [Fact] public Task RenderDoubleValueAsync()=>TestSingleElementAsync(3.14, "3.14");
    [Fact] public Task RenderIntegerValueAsync()=>TestSingleElementAsync(314, "314");
    [Fact] public Task RenderTrueValueAsync()=>TestSingleElementAsync(PdfBoolean.True, "true");
    [Fact] public Task RenderFalseValueAsync()=>TestSingleElementAsync(PdfBoolean.False, "false");
    [Fact] public Task RenderNullValueAsync()=>TestSingleElementAsync(PdfTokenValues.Null, "null");
    [Fact] public Task RenderStringValueAsync()=>TestSingleElementAsync(PdfString.CreateAscii("Foo"), "(Foo)");
    [Fact] public Task RenderSpecialtringValueAsync()=>TestSingleElementAsync(PdfString.CreateAscii("o\no"), "(o\no)");

    [Fact]
    public async Task ParseFourPagesAsync()
    {
        var builder = new PdfDocumentCreator();
        builder.Pages.CreatePage();
        builder.Pages.CreatePage();
        builder.Pages.CreatePage();
        builder.Pages.CreatePage();
        builder.CreateDocument();
        var doc = await CreateParsedFileAsync(builder.LowLevelCreator);
        Assert.Equal(new CrossReference(3,0), await doc.Pages.PageForNumberAsync(0));
        Assert.Equal(new CrossReference(4,0), await doc.Pages.PageForNumberAsync(1));
        Assert.Equal(new CrossReference(5,0), await doc.Pages.PageForNumberAsync(2));
        Assert.Equal(new CrossReference(6,0), await doc.Pages.PageForNumberAsync(3));
        
    }
}