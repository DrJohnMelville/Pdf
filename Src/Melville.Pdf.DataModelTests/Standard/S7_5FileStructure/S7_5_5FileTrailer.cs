using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S7_5_5FileTrailer
    {
        [Fact]
        public async Task ReadSingleTrailer()
        {
            var doc = await MinimalPdfGenerator.MinimalPdf(1, 7).ParseDocumentAsync(2);
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
%%EOF").ParseDocumentAsync(2);
            Assert.NotNull(doc.TrailerDictionary);
            Assert.IsType<PdfDictionary>(doc.TrailerDictionary);
            Assert.Equal("10", doc.TrailerDictionary[KnownNames.Size].ToString());
            
        }

        [Theory]
        [InlineData("%PDF1.5\r\nHeader tag is wrong.")]
        [InlineData("%PdF-1.5\r\nHeader tag is wrong.")]
        [InlineData("%PDF-1/5\r\nHeader tag is wrong.")]
        [InlineData("%PDF-X.5\r\nHeader tag is wrong.")]
        [InlineData("%PDF-1.X\r\nHeader tag is wrong.")]
        [InlineData("%PDF-1.5\r\nHas no t%railer")]
        [InlineData("%PDF-1.5\r\nHas the word trailer but not a valid dictonary")]
        [InlineData("%PDF-1.5\r\nHas the word trailer")]
        public Task MalformedPDFFiles(string text)
        {
            return Assert.ThrowsAsync<PdfParseException>(() => 
                text.ParseDocumentAsync());
        }
    }
}