using System.IO;
using System.Threading.Tasks;
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
        public async Task SimpleModification()
        {
            var doc = await LoadedDocument();
            Assert.Equal("true", (await doc.TrailerDictionary[KnownNames.Root]).ToString());
            
        }
    }
}