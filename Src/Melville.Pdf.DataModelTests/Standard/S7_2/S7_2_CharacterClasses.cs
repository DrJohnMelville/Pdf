using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_2;

public class S7_2_CharacterClasses
{
    [Theory]
    [InlineData("[true false] 1")]
    [InlineData("[    true%this is a / % comment true\rfalse] 1")]
    [InlineData("[    true%t\nfalse ]1")]
    [InlineData("[    true%\r\nfalse   ]1")]
    [InlineData("[    true%this is a / % comment true\r\r\r\r\nfalse ]1")]
    public async Task CommentAsync(string twoBoolString)
    {
        var arr = (await (await twoBoolString.ParseValueObjectAsync()).LoadValueAsync()).Get<PdfArray>();
        Assert.Equal(2, arr.RawItems.Count);
        Assert.True(arr.RawItems[0].TryGetEmbeddedDirectValue(out bool ret) && ret);
        Assert.True(arr.RawItems[1].TryGetEmbeddedDirectValue(out bool ret2) && !ret2);
    }
}