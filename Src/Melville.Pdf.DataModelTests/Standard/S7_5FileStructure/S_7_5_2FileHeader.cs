using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure
{
    public class S_7_5_2FileHeader
    {
        [Theory]
        [InlineData("%PDF-1.0\r\n ", 1, 0)]
        [InlineData("%PDF-1.1\r\n ", 1, 1)]
        [InlineData("%PDF-1.2\r\n ", 1, 2)]
        [InlineData("%PDF-1.3\r\n ", 1, 3)]
        [InlineData("%PDF-1.4\r\n ", 1, 4)]
        [InlineData("%PDF-1.5\r\n ", 1, 5)]
        [InlineData("%PDF-1.6\r\n ", 1, 6)]
        [InlineData("%PDF-1.7\r\n ", 1, 7)]
        public async Task RecognizeFileVersion(string input, int major, int minor)
        {
            var doc = await input.ParseDocumentAsync();
            Assert.Equal(major, doc.MajorVersion);
            Assert.Equal(minor, doc.MinorVersion);
            
        }

        [Fact]
        public async Task NotStartingAtZeroIsAnError()
        {
            var context = "%PDF-1.0\r\n ".AsParsingSource();
            var result = await context.ReadAsync();
            context.AdvanceTo(result.Buffer.GetPosition(1));
            await Assert.ThrowsAsync<PdfParseException>(()=>new RandomAccessFileParser(context).Parse());
        }
        
        // check that we start at the beginning
    }
}