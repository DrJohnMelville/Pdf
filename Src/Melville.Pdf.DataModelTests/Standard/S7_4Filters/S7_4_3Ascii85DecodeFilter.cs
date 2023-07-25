using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem(@"\0\0\0\0", "z", "FourZeros")]
[MacroItem(@"\0", "!!", "OneZero")]
[MacroItem(@"\x1", "!<","OneChar")]
[MacroItem(@"\x1\x1", "!<E", "TwoChars")]
[MacroItem(@"\x1\x1\x1", "!<E3", "ThreeChars")]
[MacroItem(@"\x1\x1\x1\x1", "!<E3%", "FourChars")]
[MacroItem(@"\xA", "$3", "OneA")]
[MacroItem(@"\xA\xA", "$46", "TwoAs")]
[MacroItem(@"\xA\xA\xA", "$47+", "ThreeAs")]
[MacroItem(@"\xA\xA\xA\xA", "$47+I", "FourAs")]
[MacroItem(@"d", "A,", "OneD")]
[MacroItem(@"dd", "A7P", "TwoDs")]
[MacroItem(@"ddd", "A7T3", "ThreeDs")]
[MacroItem(@"dddd", "A7T4]", "FourDs")]
[MacroItem(@"\xFF\xFF\xFF\xFF", "s8W-!", "AllOnes")]
[MacroItem(@"dddd\0\0\0\0dddd", "A7T4]zA7T4]", "EmbeddedZeros")]
[MacroItem(@"", "", "Empty")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\"+\"~>\", KnownNames.ASCII85DecodeTName){}}")]
public partial class S7_4_3Ascii85DecodeFilter
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
    public async Task EncodeStringAsync(string plain, string encoded)
    {
        await SpecialCasesAsync(plain, encoded);
        await SpecialCasesAsync(plain, encoded+"~>this garbage does not matter");
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
    public Task SpecialCasesAsync(string decoded, string encoded) =>
        StreamTest.TestContentAsync(encoded, decoded, KnownNames.ASCII85DecodeTName, PdfTokenValues.Null);
}