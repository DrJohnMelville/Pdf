using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Xml.Schema;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_7ObjectStreams
    {
        [Fact]
        public async Task CreateWithObjectStreaM()
        {
            var fileAsString = await DocWithObjectStream();
            Assert.Equal("SS", fileAsString);
        }

        private static async Task<string?> DocWithObjectStream()
        {
            var builder = new LowLevelDocumentCreator();
            builder.Add(await builder.NewObjectStream(
                builder.AsIndirectReference(new PdfString("Two")),
                builder.AsIndirectReference(new PdfString("Three"))
            ));
            var fileAsString = await DocCreatorToString(builder);
            return fileAsString;
        }

        private static async Task<string?> DocCreatorToString(LowLevelDocumentCreator? builder)
        {
            var ms = new MultiBufferStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(ms));
            await writer.WriteWithReferenceStream(builder.CreateDocument());
            var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
            return fileAsString;
        }
    }
}