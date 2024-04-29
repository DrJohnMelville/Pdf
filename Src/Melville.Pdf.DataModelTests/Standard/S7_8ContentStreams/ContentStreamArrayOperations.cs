using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_8ContentStreams;

public class ContentStreamArrayOperations
{
    private async Task DoTestAsync(string result, params string[] values)
    {
        var array = new PdfArray(values.Select(i =>
            (PdfIndirectObject)(PdfDirectObject)new DictionaryBuilder().AsStream(i)).ToArray());
        var concatStr = new PdfArrayConcatStream(array);
        var csStream = await new StreamReader(concatStr).ReadToEndAsync();
        csStream.Should().Be(result);
    }

    [Fact]
    public Task EmptyAsync() => DoTestAsync("");
    [Fact]
    public Task SingleAsync() => DoTestAsync("Hello", "Hello");
    [Fact]
    public Task DoubleAsync() => DoTestAsync("Hello\nWorld", "Hello","World");
    [Fact]
    public Task ManyAsync() => DoTestAsync("Hello\nWorld\nFoo\nBar\nBaz", "Hello","World",
        "Foo", "Bar", "Baz");
}