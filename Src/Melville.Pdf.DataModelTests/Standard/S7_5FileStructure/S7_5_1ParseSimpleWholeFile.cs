using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_1ParseSimpleWholeFile
    {
        private async Task<string> Write(PdfLowLevelDocument doc)
        {
            var target = new MemoryStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(target));
            await writer.WriteAsync(doc);
            return target.ToArray().ExtendedAsciiString();
        }
        private async Task<string> OutputTwoItemDocument(byte majorVersion = 1, byte minorVersion = 7)
        {
            var builder = new LowLevelDocumentBuilder();
            builder.SetVersion(majorVersion, minorVersion);
            builder.AddRootElement(builder.NewDictionary((KnownNames.Type, KnownNames.Catalog)));
            builder.AsIndirectReference(PdfBoolean.True); // includes a dead object to be skipped
            builder.Add(builder.NewDictionary((KnownNames.Type, KnownNames.Page)));
            return await Write(builder.CreateDocument());
        }

        private async Task RoundTripPdfAsync(string pdf)
        {
            var doc = await pdf.ParseDocumentAsync();
            var newPdf = await Write(doc);
            Assert.Equal(pdf, newPdf);
            
        }

        [Fact]
        public async Task RoundTripSimpleDocument()
        {
            await RoundTripPdfAsync(await OutputTwoItemDocument(1, 3));

        }

        [Fact]
        public async Task ParseSimpleDocument()
        {
            var doc = await (await OutputTwoItemDocument()).ParseDocumentAsync();
            Assert.Equal(1, doc.MajorVersion);
            Assert.Equal(7, doc.MinorVersion);
            Assert.Equal(2, doc.Objects.Count);
            Assert.Equal(2, ((PdfNumber)(await doc.TrailerDictionary[KnownNames.Size])).IntValue);
            
            
            
        }
    }
}