using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.LowLevelReader.DocumentParts;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader
{
    public class PartParserTest
    {
        private readonly Mock<IWaitingService> waitingService = new();
        
        private readonly PartParser sut = new(Mock.Of<IPasswordSource>());
        [Theory]
        [InlineData(1,7)]
        [InlineData(1,3)]
        [InlineData(2,0)]
        public async Task ParseHeader(int major, int minor)
        {
            var model =  await sut.ParseAsync(
                await MinimalPdfParser.MinimalPdf(major, minor).AsFileAsync(),waitingService.Object);
            Assert.Equal($"PDF-{major}.{minor}", model[0].Title);
        }

        [Fact]
        public async Task ParseFinalDictionary()
        {
            var model = await sut.ParseAsync(
                await MinimalPdfParser.MinimalPdf(1, 7).AsFileAsync(),
                waitingService.Object);
            var trailerNode = model.Last();
            Assert.Equal("Trailer: Dictionary", trailerNode.Title);
            Assert.Equal(2, trailerNode.Children.Count);
            Assert.Equal("/Size: 5", trailerNode.Children[1].Title);
            Assert.Equal("/Root: 4 0 R", trailerNode.Children[0].Title);
        }

        [Fact]
        public async Task ReportWaitingTime()
        {
            var model = await sut.ParseAsync(
                await MinimalPdfParser.MinimalPdf(1, 7).AsFileAsync(),
                waitingService.Object);
            waitingService.Verify(i=>i.WaitBlock("Loading File", 5, false), Times.Once);
            waitingService.Verify(i=>i.MakeProgress(It.IsAny<string?>()), Times.Exactly(5));
            waitingService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ParseArray()
        {
            var model = await BuildSingleElementFile(_=>
                new PdfArray(PdfBoolean.True, PdfBoolean.False, KnownNames.Max));
            var array = model[2].Children[0];
            Assert.Equal("Array", array.Title);
            Assert.Equal("[0]: true", array.Children[0].Title);
            Assert.Equal("[1]: false", array.Children[1].Title);
            Assert.Equal("[2]: /Max", array.Children[2].Title);
        }
        [Fact]
        public async Task ParseSteam()
        {
            var model = await BuildSingleElementFile(i=>
                i.NewStream("The Stream Data", (KnownNames.Type, KnownNames.Page)));
            var stream = (StreamDocumentPart)model[2].Children[0];
            await stream.LoadBytesAsync();
            Assert.Equal("Stream", stream.Title);
            Assert.Equal("/Type: /Page", stream.Children[0].Title);
            Assert.Equal("/Length: 15", stream.Children[1].Title);
            Assert.Equal("00000000  54 68 65 20 53 74 72 65 61 6D 20 44 61 74 61      The Stream Data", stream.DisplayContent);
        }

        private async Task TestSingleElement(PdfObject item, string renderAs)
        {
            var model = await BuildSingleElementFile(_=>item);
            Assert.Equal("1 0 obj", model[2].Title);
            Assert.Equal(renderAs.Length, model[2].Children[0].Title.Length);
            Assert.Equal(renderAs, model[2].Children[0].Title);
        }

        private async Task<DocumentPart[]> BuildSingleElementFile(
            Func<ILowLevelDocumentCreator,PdfObject> item)
        {
            var builder = new LowLevelDocumentCreator();
            builder.Add(item(builder));
            return await sut.ParseAsync(await builder.AsFileAsync(), waitingService.Object);
        }

        [Fact] public Task RenderDoubleValue()=>TestSingleElement(new PdfDouble(3.14), "3.14");
        [Fact] public Task RenderIntegerValue()=>TestSingleElement(new PdfInteger(314), "314");
        [Fact] public Task RenderTrueValue()=>TestSingleElement(PdfBoolean.True, "true");
        [Fact] public Task RenderFalseValue()=>TestSingleElement(PdfBoolean.False, "false");
        [Fact] public Task RenderNullValue()=>TestSingleElement(PdfTokenValues.Null, "null");
        [Fact] public Task RenderStringValue()=>TestSingleElement(PdfString.CreateAscii("Foo"), "(Foo)");
        [Fact] public Task RenderSpecialtringValue()=>TestSingleElement(PdfString.CreateAscii("o\no"), "(o\no)");
        
    }
}