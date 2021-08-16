using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
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
        [Fact]
        public Task BugCheck() =>
            StreamTest.Encoding(KnownNames.ASCIIHexDecode, null, 
                "\x16\xc0\xa0\x44\x18\x19\x0a\x02", "16C0A04418190A02");

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