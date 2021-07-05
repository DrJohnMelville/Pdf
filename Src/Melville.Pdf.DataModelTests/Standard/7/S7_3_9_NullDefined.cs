using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_9_NullDefined
    {
        private static bool ParseNull(out PdfObject? nullObj)
        {
            return "null\\".ParseAs(out nullObj);
        }

        [Fact]
        public void CanParseNull()
        {
            var succeed = ParseNull(out var nullObj);
            Assert.True(succeed);
            Assert.Equal(PdfNull.Instance, nullObj);
            
        }

        [Fact]
        public void NullIsASingleton()
        {
            ParseNull(out var n1);
            ParseNull(out var n2);
            Assert.Equal(n1, n2);
            
        }
    }
}