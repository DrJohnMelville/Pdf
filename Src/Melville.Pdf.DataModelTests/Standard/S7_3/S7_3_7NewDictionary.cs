using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7NewDictionary
{
    private readonly PdfValueDictionary sut = new ValueDictionaryBuilder()
        .WithItem("/A"u8, 1)
        .WithItem("/B"u8, "String")
        .WithItem("/C"u8, true)
        .AsDictionary();
    
    [Fact]
    public void TestCount() => Assert.Equal(3, sut.Count);

    [Fact]
    public async Task TestRawAccessAsync() => 
        Assert.Equal(1, (await sut.RawItems["/A"].LoadValueAsync()).Get<int>());

    [Theory]
    [InlineData("/A", true)]
    [InlineData("/B", true)]
    [InlineData("/C", true)]
    [InlineData("/D", false)]
    public void TestContainsKey(string key, bool contained) =>
        Assert.Equal(contained, sut.ContainsKey(key));

    [Theory]
    [InlineData("/A", true, "1")]
    [InlineData("/B", true, "String")]
    [InlineData("/C", true, "true")]
    [InlineData("/D", false, "")]
    public async Task TestTryGetValueAsync(string key, bool contained, string value)
    {
        var found = sut.TryGetValue(key, out var foundVal);
        Assert.Equal(contained, found);
        if (found)
            Assert.Equal(value, (await foundVal).ToString());
    }

    [Theory]
    [InlineData("/A", true, "1")]
    [InlineData("/B", true, "String")]
    [InlineData("/C", true, "true")]
    [InlineData("/D", false, "")]
    public async Task TestIndexerAsync(string key, bool contained, string value)
    {
        if (contained)
            Assert.Equal(value, (await sut[key]).ToString());
        else
            Assert.Throws<KeyNotFoundException>(() => sut[key]);
    }

    [Fact]
    public void TestKeys()
    {
        var keys = sut.Keys;
        Assert.Equal("A B C", string.Join(" ", keys));
    }

    [Fact]
    public async Task TestValuesAsync()
    {
        var keys = sut.Values;
        var sb = new StringBuilder();
        foreach (var value in keys)
        {
            sb.Append(await value);
            sb.Append(" ");
        }
        Assert.Equal("1 String true ", sb.ToString());
    }
}