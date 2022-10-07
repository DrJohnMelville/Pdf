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
    public async Task ParseBoolSucceed(string text, bool value)
    {
        var item = (PdfBoolean) await text.ParseObjectAsync();
        Assert.Equal(value, item.Value);
        Assert.True(ReferenceEquals(value?PdfBoolean.True:PdfBoolean.False,item));
    }

    [Theory]
    [InlineData("tRue")]
    [InlineData("tunk")]
    [InlineData("fAlse")]
    public Task LiteralNamesMustBeSpelledCorrectly(string s)
    {
        return Assert.ThrowsAsync<PdfParseException>(s.ParseObjectAsync);
    }
}