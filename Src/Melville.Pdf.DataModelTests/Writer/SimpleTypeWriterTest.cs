using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Moq;

namespace Melville.Pdf.DataModelTests.Writer;

public class SimpleTypeWriterTest
{
    [Test]
    public async Task WriteTokensAsync()
    {
        Assert.Equal("true",await ((PdfIndirectObject)true).WriteToStringAsync());
        Assert.Equal("false",await ((PdfIndirectObject)false).WriteToStringAsync());
        Assert.Equal("null",await ((PdfIndirectObject)default).WriteToStringAsync());
    }
        
    [Test]
    [Arguments("Hello", "(Hello)")]
    [Arguments("", "()")]
    [Arguments("\n", "(\\n)")]
    [Arguments("\r", "(\\r)")]
    [Arguments("\t", "(\\t)")]
    [Arguments("\b", "(\\b)")]
    [Arguments("\f", "(\\f)")]
    [Arguments("\u0000", "(\u0000)")]
    [Arguments(@"this is a \Test", @"(this is a \\Test)")]
    [Arguments(@"this is a (Test", @"(this is a \(Test)")]
    [Arguments(@"this is a )Test", @"(this is a \)Test)")]
    public async Task WriteStringsAsync(string source, string dest)
    {
        Assert.Equal(dest, await PdfDirectObject.CreateString(source.AsExtendedAsciiBytes()).WriteToStringAsync());
        Assert.Equal(source, (await dest.ParseValueObjectAsync()).ToString());
            
    }
    [Test]
    [Arguments("Hello", "/Hello")]
    [Arguments("Hel#lo", "/Hel#23lo")]
    [Arguments("Hel lo", "/Hel#20lo")]
    public async Task WriteNameAsync(string source, string dest)
    {
        Assert.Equal(dest, await PdfDirectObject.CreateName(source).WriteToStringAsync());
    }
    [Test]
    [Arguments(0, "0")]
    [Arguments(1234, "1234")]
    [Arguments(-1234, "-1234")]
    public async Task WriteIntegersAsync(int source, string dest)
    {
        Assert.Equal(dest, await ((PdfDirectObject)source).WriteToStringAsync());
    }
    [Test]
    [Arguments(0, "0")]
    [Arguments(1234, "1234")]
    [Arguments(-1234, "-1234")]
    [Arguments(-1234.54, "-1234.54")]
    public async Task WriteDoublesAsync(double source, string dest)
    {
        Assert.Equal(dest, await ((PdfDirectObject)source).WriteToStringAsync());
    }

    [Test]
    public async Task WriteIndirectObjectReferenceAsync()
    {
        var reference = new PdfIndirectObject(Mock.Of<IIndirectObjectSource>(), 34,555);
        Assert.Equal("34 555 R", await reference.WriteToStringAsync());

    }
    [Test]
    public async Task WriteArrayAsync()
    {
        var array = new PdfArray(new PdfIndirectObject[]
        {
            true, false, default
        });
        Assert.Equal("[true false null]", await ((PdfDirectObject)array).WriteToStringAsync());
    }
    [Test]
    public async Task WriteDictionaryAsync()
    {
        var array =
            new DictionaryBuilder()
                .WithItem(KnownNames.Width, 20)
                .WithItem(KnownNames.Height, 40)
                .AsDictionary();
        Assert.Equal("<</Width 20/Height 40>>", await ((PdfDirectObject)array).WriteToStringAsync());
    }

    [Test]
    public async Task WriteStreamAsync()
    {
        var array = new DictionaryBuilder()
            .WithItem(KnownNames.Length, 5).AsStream("Hello");
        Assert.Equal("<</Length 5>> stream\r\nHello\r\nendstream", await ((PdfDirectObject)array).WriteStreamToStringAsync());
    }
}