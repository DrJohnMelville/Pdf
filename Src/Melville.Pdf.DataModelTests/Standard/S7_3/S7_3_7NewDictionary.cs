using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3_7NewDictionary
{
    private readonly PdfDictionary sut = new DictionaryBuilder()
        .WithItem("/A"u8, 1)
        .WithItem("/B"u8, "String")
        .WithItem("/C"u8, true)
        .AsDictionary();
    
    [Test]
    public void TestCount() => Assert.Equal(3, sut.Count);

    [Test]
    public async Task TestRawAccessAsync() => 
        Assert.Equal(1, (await sut.RawItems["/A"].LoadValueAsync()).Get<int>());

    [Test]
    [Arguments("/A", true)]
    [Arguments("/B", true)]
    [Arguments("/C", true)]
    [Arguments("/D", false)]
    public void TestContainsKey(string key, bool contained) =>
        Assert.Equal(contained, sut.ContainsKey(key));

    [Test]
    [Arguments("/A", true, "1")]
    [Arguments("/B", true, "String")]
    [Arguments("/C", true, "true")]
    [Arguments("/D", false, "")]
    public async Task TestTryGetValueAsync(string key, bool contained, string value)
    {
        var found = sut.TryGetValue(key, out var foundVal);
        Assert.Equal(contained, found);
        if (found)
            Assert.Equal(value, (await foundVal).ToString());
    }

    [Test]
    [Arguments("/A", true, "1")]
    [Arguments("/B", true, "String")]
    [Arguments("/C", true, "true")]
    [Arguments("/D", false, "")]
    public async Task TestIndexerAsync(string key, bool contained, string value)
    {
        if (contained)
            Assert.Equal(value, (await sut[key]).ToString());
        else
            key.Invoking(i => sut[i]).Should().Throw<KeyNotFoundException>();
    }

    [Test]
    public void TestKeys()
    {
        var keys = sut.Keys;
        Assert.Equal("A B C", string.Join(" ", keys));
    }

    [Test]
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