using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_5FileTrailer
    {
        [Fact]
        public async Task ReadSingleTrailer()
        {
            var doc = await MinimalPdfGenerator.MinimalPdf(1, 7).ParseDocumentAsync();
            Assert.NotNull(doc.TrailerDictionary);
            Assert.IsType<PdfDictionary>(doc.TrailerDictionary);
            
        }
        [Fact]
        public async Task ReadOnlyLastTrailer()
        {
            var doc =  await (MinimalPdfGenerator.MinimalPdf(1, 7)+@"trailer
<< /Size 10 /Root 1 0 R >>
startxref
394
%%EOF").ParseDocumentAsync();
            Assert.NotNull(doc.TrailerDictionary);
            Assert.IsType<PdfDictionary>(doc.TrailerDictionary);
            Assert.Equal("10", doc.TrailerDictionary[KnownNames.Size].ToString());
            
        }
    }
}