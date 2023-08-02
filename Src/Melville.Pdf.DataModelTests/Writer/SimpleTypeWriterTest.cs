using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer;

public class SimpleTypeWriterTest
{
    [Fact]
    public async Task WriteTokensAsync()
    {
        Assert.Equal("true",await ((PdfIndirectObject)true).WriteToStringAsync());
        Assert.Equal("false",await ((PdfIndirectObject)false).WriteToStringAsync());
        Assert.Equal("null",await ((PdfIndirectObject)default).WriteToStringAsync());
    }
        
    [Theory]
    [InlineData("Hello", "(Hello)")]
    [InlineData("", "()")]
    [InlineData("\n", "(\\n)")]
    [InlineData("\r", "(\\r)")]
    [InlineData("\t", "(\\t)")]
    [InlineData("\b", "(\\b)")]
    [InlineData("\f", "(\\f)")]
    [InlineData("\u0000", "(\u0000)")]
    [InlineData(@"this is a \Test", @"(this is a \\Test)")]
    [InlineData(@"this is a (Test", @"(this is a \(Test)")]
    [InlineData(@"this is a )Test", @"(this is a \)Test)")]
    public async Task WriteStringsAsync(string source, string dest)
    {
        Assert.Equal(dest, await PdfDirectObject.CreateString(source.AsExtendedAsciiBytes()).WriteToStringAsync());
        Assert.Equal(source, (await dest.ParseValueObjectAsync()).ToString());
            
    }
    [Theory]
    [InlineData("Hello", "/Hello")]
    [InlineData("Hel#lo", "/Hel#23lo")]
    [InlineData("Hel lo", "/Hel#20lo")]
    public async Task WriteNameAsync(string source, string dest)
    {
        Assert.Equal(dest, await PdfDirectObject.CreateName(source).WriteToStringAsync());
    }
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1234, "1234")]
    [InlineData(-1234, "-1234")]
    public async Task WriteIntegersAsync(int source, string dest)
    {
        Assert.Equal(dest, await ((PdfDirectObject)source).WriteToStringAsync());
    }
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1234, "1234")]
    [InlineData(-1234, "-1234")]
    [InlineData(-1234.54, "-1234.54")]
    public async Task WriteDoublesAsync(double source, string dest)
    {
        Assert.Equal(dest, await ((PdfDirectObject)source).WriteToStringAsync());
    }

    [Fact]
    public async Task WriteIndirectObjectReferenceAsync()
    {
        var reference = new PdfIndirectObject(Mock.Of<IIndirectObjectSource>(), 34,555);
        Assert.Equal("34 555 R", await reference.WriteToStringAsync());

    }
    [Fact]
    public async Task WriteArrayAsync()
    {
        var array = new PdfArray(new PdfIndirectObject[]
        {
            true, false, default
        });
        Assert.Equal("[true false null]", await ((PdfDirectObject)array).WriteToStringAsync());
    }
    [Fact]
    public async Task WriteDictionaryAsync()
    {
        var array =
            new DictionaryBuilder()
                .WithItem(KnownNames.WidthTName, 20)
                .WithItem(KnownNames.HeightTName, 40)
                .AsDictionary();
        Assert.Equal("<</Width 20/Height 40>>", await ((PdfDirectObject)array).WriteToStringAsync());
    }

    [Fact]
    public async Task WriteStreamAsync()
    {
        var array = new DictionaryBuilder()
            .WithItem(KnownNames.LengthTName, 5).AsStream("Hello");
        Assert.Equal("<</Length 5>> stream\r\nHello\r\nendstream", await ((PdfDirectObject)array).WriteStreamToStringAsync());
    }
}