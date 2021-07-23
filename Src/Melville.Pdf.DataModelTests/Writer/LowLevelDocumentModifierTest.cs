using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.DataModelTests.PdfStreamHolderTest;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class LowLevelDocumentModifierTest
    {
        private readonly PdfLowLevelDocument baseDoc;

        public LowLevelDocumentModifierTest()
        {
            var builder = new LowLevelDocumentCreator();
            var rootref = builder.Add(PdfBoolean.True);
            builder.Add(PdfBoolean.False);
            builder.AddToTrailerDictionary(KnownNames.Root, rootref);
            baseDoc = builder.CreateDocument();
        }

        private async Task<PdfLoadedLowLevelDocument> LoadedDocument()
        {
            var ms = new MemoryStream();
            await baseDoc.WriteTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return await RandomAccessFileParser.Parse(ms);
        }

        [Fact]
        public async Task NullModification()
        {
            var target = new TestWriter();
            var doc = await LoadedDocument();
            var sut = new LowLevelDocumentModifier(doc);
            await sut.WriteModificationTrailer(target.Writer, 0);
            Assert.Equal("xref\n", target.Result());
        }
    }
}