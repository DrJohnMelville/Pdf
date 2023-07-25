using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer;

public class SimpleTypeWriterTest
{
    [Fact]
    public async Task WriteTokensAsync()
    {
        Assert.Equal("true",await true.WriteToStringAsync());
        Assert.Equal("false",await false.WriteToStringAsync());
        Assert.Equal("null",await PdfTokenValues.Null.WriteToStringAsync());
        Assert.Equal("]",await PdfTokenValues.ArrayTerminator.WriteToStringAsync());
        Assert.Equal(">>",await PdfTokenValues.DictionaryTerminator.WriteToStringAsync());
            
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
        Assert.Equal(dest, await PdfString.CreateAscii(source).WriteToStringAsync());
        Assert.Equal(source, (await dest.ParseObjectAsync()).ToString());
            
    }
    [Theory]
    [InlineData("Hello", "/Hello")]
    [InlineData("Hel#lo", "/Hel#23lo")]
    [InlineData("Hel lo", "/Hel#20lo")]
    public async Task WriteNameAsync(string source, string dest)
    {
        Assert.Equal(dest, await PdfDirectValue.CreateName(source).WriteToStringAsync());
    }
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1234, "1234")]
    [InlineData(-1234, "-1234")]
    public async Task WriteIntegersAsync(int source, string dest)
    {
        Assert.Equal(dest, await new PdfInteger(source).WriteToStringAsync());
    }
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1234, "1234")]
    [InlineData(-1234, "-1234")]
    [InlineData(-1234.54, "-1234.54")]
    public async Task WriteDoublesAsync(double source, string dest)
    {
        Assert.Equal(dest, await ((PdfDouble)source).WriteToStringAsync());
    }

    [Fact]
    public async Task WriteIndirectObjectReferenceAsync()
    {
        var reference = new PdfIndirectObject(34, 555, false);
        Assert.Equal("34 555 R", await reference.WriteToStringAsync());

    }
    [Fact]
    public async Task WriteArrayAsync()
    {
        var array = new PdfValueArray(new[]
        {
            true, false, PdfTokenValues.Null
        });
        Assert.Equal("[true false null]", await array.WriteToStringAsync());
    }
    [Fact]
    public async Task WriteDictionaryAsync()
    {
        var array =
            new ValueDictionaryBuilder()
                .WithItem(KnownNames.WidthTName, 20)
                .WithItem(KnownNames.HeightTName, 40)
                .AsDictionary();
        Assert.Equal("<</Width 20/Height 40>>", await array.WriteToStringAsync());
    }

    [Fact]
    public async Task WriteStreamAsync()
    {
        var array = new ValueDictionaryBuilder()
            .WithItem(KnownNames.LengthTName, 5).AsStream("Hello");
        Assert.Equal("<</Length 5>> stream\r\nHello\r\nendstream", await array.WriteToStringAsync());
    }
}