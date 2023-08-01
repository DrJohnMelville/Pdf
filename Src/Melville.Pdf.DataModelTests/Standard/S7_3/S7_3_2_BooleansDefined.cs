using System.Formats.Tar;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public sealed class S7_3_2_BooleansDefined
{
    [Theory]
    [InlineData("true /", true)]
    [InlineData("false /", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public async Task ParseBoolSucceedAsync(string text, bool value)
    {
        var item = await text.ParseValueObjectAsync();
        Assert.Equal(value, await item.LoadValueAsync<bool>());
    }

    [Theory]
    [InlineData("tRue")]
    [InlineData("tunk")]
    [InlineData("fAlse")]
    public Task LiteralNamesMustBeSpelledCorrectlyAsync(string s)
    {
        return Assert.ThrowsAsync<PdfParseException>(()=>s.ParseValueObjectAsync().AsTask());
    }
}