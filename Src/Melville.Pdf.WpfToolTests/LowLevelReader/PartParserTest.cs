using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.LowLevelReader.DocumentParts;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelReader
{
    public class PartParserTest
    {
        private readonly PartParser sut = new();
        [Theory]
        [InlineData(1,7)]
        [InlineData(1,3)]
        [InlineData(2,0)]
        public async Task ParseHeader(int major, int minor)
        {
            var model = await sut.ParseAsync(await MinimalPdfGenerator.MinimalPdf(major, minor).AsFileAsync());
            Assert.Equal($"PDF-{major}.{minor}", model[0].Title);
        }

        [Fact]
        public async Task ParseFinalDictionary()
        {
            var model = await sut.ParseAsync(await MinimalPdfGenerator.MinimalPdf(1,7).AsFileAsync());
            var trailerNode = model.Last();
            Assert.Equal("Trailer: Dictionary", trailerNode.Title);
            Assert.Equal(2, trailerNode.Children.Count);
            Assert.Equal("/Size: 7", trailerNode.Children[1].Title);
            Assert.Equal("/Root: 1 0 R", trailerNode.Children[0].Title);
        }

        [Fact]
        public async Task ParseArray()
        {
            var model = await BuildSingleElementFile(_=>
                new PdfArray(PdfBoolean.True, PdfBoolean.False, KnownNames.Max));
            var array = model[1].Children[0];
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
            var stream = (StreamDocumentPart)model[1].Children[0];
            await stream.LoadBytesAsync();
            Assert.Equal("Stream", stream.Title);
            Assert.Equal("/Type: /Page", stream.Children[0].Title);
            Assert.Equal("/Length: 15", stream.Children[1].Title);
            Assert.Equal("The Stream Data", ExtendedAsciiEncoding.ExtendedAsciiString(stream.Bytes));
        }

        private async Task TestSingleElement(PdfObject item, string renderAs)
        {
            var model = await BuildSingleElementFile(_=>item);
            Assert.Equal("1 0 obj", model[1].Title);
            Assert.Equal(renderAs, model[1].Children[0].Title);
        }

        private async Task<DocumentPart[]> BuildSingleElementFile(
            Func<ILowLevelDocumentBuilder,PdfObject> item)
        {
            var builder = new LowLevelDocumentBuilder();
            builder.Add(item(builder));
            return await sut.ParseAsync(await builder.AsFileAsync());
        }

        [Fact] public Task RenderDoubleValue()=>TestSingleElement(new PdfDouble(3.14), "3.14");
        [Fact] public Task RenderIntegerValue()=>TestSingleElement(new PdfInteger(314), "314");
        [Fact] public Task RenderTrueValue()=>TestSingleElement(PdfBoolean.True, "true");
        [Fact] public Task RenderFalseValue()=>TestSingleElement(PdfBoolean.False, "false");
        [Fact] public Task RenderNullValue()=>TestSingleElement(PdfTokenValues.Null, "null");
        [Fact] public Task RenderStringValue()=>TestSingleElement(new PdfString("Foo"), "(Foo)");
        
    }
}