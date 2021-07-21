using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
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
            var model = await sut.Parse(await MinimalPdfGenerator.MinimalPdf(major, minor).AsFile());
            Assert.Equal($"PDF-{major}.{minor}", model[0].Title);
        }

        [Fact]
        public async Task ParseFinalDictionary()
        {
            var model = await sut.Parse(await MinimalPdfGenerator.MinimalPdf(1,7).AsFile());
            var trailerNode = model.Last();
            Assert.Equal("Trailer: Dictionary", trailerNode.Title);
            Assert.Equal(2, trailerNode.Children.Count);
            Assert.Equal("/Size: 7", trailerNode.Children[1].Title);
            Assert.Equal("/Root: 1 0 R", trailerNode.Children[0].Title);
        }

        private async Task TestSingleElement(PdfObject item, string renderAs)
        {
            var builder = new LowLevelDocumentBuilder();
            builder.Add(item);
            var model = await sut.Parse(await builder.AsFile());
            Assert.Equal("1 0 obj", model[1].Title);
            Assert.Equal(renderAs, model[1].Children[0].Title);
        }

        [Fact] public Task RenderDoubleValue()=>TestSingleElement(new PdfDouble(3.14), "3.14");
        [Fact] public Task RenderIntegerValue()=>TestSingleElement(new PdfInteger(314), "314");
        [Fact] public Task RenderTrueValue()=>TestSingleElement(PdfBoolean.True, "true");
        [Fact] public Task RenderFalseValue()=>TestSingleElement(PdfBoolean.False, "false");
        [Fact] public Task RenderNullValue()=>TestSingleElement(PdfTokenValues.Null, "null");
        // finish testing the render visitor
    }
}