using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("Hello World.", "48656C6C6F20576F726C642E", "HelloWorld")]
[MacroItem(@"\x16\xc0\xa0\x44\x18\x19\x0a\x02", "16C0A04418190A02", "HexDecodeBugCheck")]
[MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecodeTName){}}")]
public partial class S7_4_2AsciiHexDecodeFilter
{
    [Theory]
    [InlineData("2020", "  ")]
    [InlineData("7>70", "p")]
    [InlineData("2020>", "  ")]
    [InlineData("6464>", "dd")]
    [InlineData("202>", "  ")]
    [InlineData("707", "pp")]
    [InlineData("20 \r\n\t 20", "  ")]
    public Task SpecialCasesAsync(string encoded, string decoded) =>
        StreamTest.TestContentAsync(encoded, decoded, KnownNames.ASCIIHexDecodeTName, default);

}