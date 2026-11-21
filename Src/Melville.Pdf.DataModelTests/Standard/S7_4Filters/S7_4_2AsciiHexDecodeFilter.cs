using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("Hello World.", "48656C6C6F20576F726C642E", "HelloWorld")]
[MacroItem(@"\x16\xc0\xa0\x44\x18\x19\x0a\x02", "16C0A04418190A02", "HexDecodeBugCheck")]
[MacroCode("[InheritsTests] public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecode){}}")]
public partial class S7_4_2AsciiHexDecodeFilter
{
    [Test]
    [Arguments("2020", "  ")]
    [Arguments("7>70", "p")]
    [Arguments("2020>", "  ")]
    [Arguments("6464>", "dd")]
    [Arguments("202>", "  ")]
    [Arguments("707", "pp")]
    [Arguments("20 \r\n\t 20", "  ")]
    public Task SpecialCasesAsync(string encoded, string decoded) =>
        StreamTest.TestContentAsync(encoded, decoded, KnownNames.ASCIIHexDecode, default);

}