using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public sealed class S7_3_2_BooleansDefined
{
    [Test]
    [Arguments("true /", true)]
    [Arguments("false /", false)]
    [Arguments("true", true)]
    [Arguments("false", false)]
    public async Task ParseBoolSucceedAsync(string text, bool value)
    {
        var item = await text.ParseValueObjectAsync();
        Assert.Equal(value, await item.LoadValueAsync<bool>());
    }

    [Test]
    [Arguments("tRue")]
    [Arguments("tunk")]
    [Arguments("fAlse")]
    public Task LiteralNamesMustBeSpelledCorrectlyAsync(string s)
    {
        return Assert.ThrowsAsync<PdfParseException>(()=>s.ParseValueObjectAsync().AsTask());
    }
}