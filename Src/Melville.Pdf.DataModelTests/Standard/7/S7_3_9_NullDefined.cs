using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_9_NullDefined
    {
        private static Task<PdfObject> ParsedNull()
        {
            return "null ".ParseToPdfAsync();
        }

        [Fact]
        public async Task CanParseNull()
        {
            Assert.Equal(PdfEmptyConstants.Null, await ParsedNull());
            
        }

        [Fact]
        public async Task NullIsASingleton()
        {
            Assert.True(ReferenceEquals(await ParsedNull(), await ParsedNull()));
            
        }
    }
}