using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_9_NullDefined
{
    [Theory]
    [InlineData("null")]
    [InlineData("null  ")]
    [InlineData("  \r\nnull  ")]
    private static async Task ParsedNullAsync(string text)
    {
        var result = await (await text.ParseValueObjectAsync()).LoadValueAsync();
        Debug.Assert(result.IsNull);
    }

}