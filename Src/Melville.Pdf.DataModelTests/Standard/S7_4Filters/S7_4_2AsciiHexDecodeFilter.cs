using System.Threading.Tasks;
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
        [InlineData("202>", "  ")]
        [InlineData("707", "pp")]
        [InlineData("20 \r\n\t 20", "  ")]
        public Task SpecialCaseqs(string encoded, string decoded) =>
            StreamTest.TestContent(encoded, decoded, new AsciiHexDecoder(), PdfTokenValues.Null);

    }

    public class S7_4_3Ascii85DecodeFilter
    {
        [Theory]
        [InlineData("\0\0\0\0", "z")]
        [InlineData("\0", "!!")]
        [InlineData("\x1", "!<")]
        [InlineData("\x1\x1", "!<E")]
        [InlineData("\x1\x1\x1", "!<E3")]
        [InlineData("\x1\x1\x1\x1", "!<E3%")]
        [InlineData("\xA", "$3")]
        [InlineData("\xA\xA", "$46")]
        [InlineData("\xA\xA\xA", "$47+")]
        [InlineData("\xA\xA\xA\xA", "$47+I")]
        [InlineData("d", "A,")]
        [InlineData("dd", "A7P")]
        [InlineData("ddd", "A7T3")]
        [InlineData("dddd", "A7T4]")]
        [InlineData("\xFF\xFF\xFF\xFF", "s8W-!")]
        [InlineData("dddd\0\0\0\0dddd", "A7T4]zA7T4]")]
        [InlineData("", "")]
        public Task EncodeString(string plain, string encoded) =>
            StreamTest.Encoding(KnownNames.ASCII85Decode, null, plain, encoded);
    }
}