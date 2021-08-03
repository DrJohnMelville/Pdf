using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.AshiiHexFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    public class S7_4_2AsciiHexDecodeFilter
    {
        [Fact]
        public Task WriteEncodedStream() =>
            StreamTest.Encoding(KnownNames.ASCIIHexDecode, null, 
                "Hello World.", "48656C6C6F20576F726C642E");

        [Theory]
        [InlineData("2020", "  ")]
        [InlineData("7>70", "p")]
        [InlineData("2020>", "  ")]
        [InlineData("6464>", "dd")]
        [InlineData("202>", "  ")]
        [InlineData("707", "pp")]
        [InlineData("20 \r\n\t 20", "  ")]
        public Task SpecialCases(string encoded, string decoded) =>
            StreamTest.TestContent(encoded, decoded, new AsciiHexDecoder(), PdfTokenValues.Null);

    }
}