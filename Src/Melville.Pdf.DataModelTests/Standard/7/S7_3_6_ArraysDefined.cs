using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard._7
{
    public class S7_3_6_ArraysDefined
    {
        [Theory]
        [InlineData("[]%", 0)]
        public void ParseArray(string src, int length)
        {
            var dat = src.AsSequenceReader();
        }
    }
}