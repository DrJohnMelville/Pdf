using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_9_NullDefined
{
    [Test]
    [Arguments("null")]
    [Arguments("null  ")]
    [Arguments("  \r\nnull  ")]
    public async Task ParsedNullAsync(string text)
    {
        var result = await (await text.ParseValueObjectAsync()).LoadValueAsync();
        Debug.Assert(result.IsNull);
    }

}