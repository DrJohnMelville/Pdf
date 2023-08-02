using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7_DictionaryDefined
{
    [Theory]
    [InlineData("  << /Height 213 /Width 456  >>  ")]
    [InlineData("<</Height 213/Width 456>>")]
    public async Task ParseSimpleDictionaryAsync(string input)
    {
        var dict = (await (await input.ParseValueObjectAsync()).LoadValueAsync())
            .Get<PdfValueDictionary>();
        Assert.Equal(2, dict.RawItems.Count);
        Assert.Equal(213, await dict.GetAsync<int>("/Height"u8));
        Assert.Equal(456, await dict.GetAsync<int>("/Width"u8));
    }

    [Theory]
    [InlineData("  << >>  ", 0)]
    [InlineData("<</Height 213 /Width 456 /ASPECT null >>", 2)] // PDF Spec  nulls make the entry be ignored
    [InlineData(" << /DICT << /InnerDict 121.22 >>>>", 1)] // dictionary can contain dictionaries
    public async Task SpecialCasesAsync(string input, int size)
    {
        var dict = (await (await input.ParseValueObjectAsync()).LoadValueAsync())
            .Get<PdfValueDictionary>();
        Assert.Equal(size, dict.RawItems.Count);
    }

    [Theory]
    [InlineData("  <<  213 /Height /Width 456  >>  ")]
    [InlineData("<</Height 213/Width>>")]
    public Task ExceptionsAsync(string input) =>
        Assert.ThrowsAsync<PdfParseException>(() => input.ParseValueObjectAsync().AsTask());

    [Fact]
    public async Task ParseRootDictionary()
    {
        var item = await " 1 2 obj<</Height 213/Width 456>>endobj".ParseRootObjectAsync();
        var dict = item.Get<PdfValueDictionary>();
        Assert.Equal(213, await dict.GetAsync<int>("/Height"u8));
        Assert.Equal(456, await dict.GetAsync<int>("/Width"u8));

    }

}