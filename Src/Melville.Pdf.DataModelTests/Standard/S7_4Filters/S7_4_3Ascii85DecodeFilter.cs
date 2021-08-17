using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
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
        public async Task EncodeString(string plain, string encoded)
        {
           await StreamTest.Encoding(KnownNames.ASCII85Decode, null, plain, encoded+"~>");
           await SpecialCases(plain, encoded);
           await SpecialCases(plain, encoded+"~>this garbage does not matter");
        }
        
        [Theory]
        [InlineData("dddd", "A7T4]")]
        [InlineData("dddd", "A7T 4]")]
        [InlineData("dddd", " A 7 T 4 ]")]
        [InlineData("dddd", " A 7 \t\r\n T 4 ]")]
        [InlineData("dddd", " A 7 \t\r\n T 4 ]    ")]
        [InlineData("dddd", " A 7 \t\r\n T 4 ]    ~>")]
        [InlineData("ddd", "A7T3  ~>")]
        [InlineData("ddd", "A7T3~> jsdhlk oky wqo' gdwqj 'ggb3eg2kph rgkj ohe3fgho' ihk tb3")]
        public Task SpecialCases(string decoded, string encoded) =>
            StreamTest.TestContent(encoded, decoded, KnownNames.ASCII85Decode, PdfTokenValues.Null);
    }
}