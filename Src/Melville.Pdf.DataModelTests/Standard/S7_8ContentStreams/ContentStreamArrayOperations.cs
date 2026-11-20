using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;

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

    [Test]
    public Task EmptyAsync() => DoTestAsync("");
    [Test]
    public Task SingleAsync() => DoTestAsync("Hello", "Hello");
    [Test]
    public Task DoubleAsync() => DoTestAsync("Hello\nWorld", "Hello","World");
    [Test]
    public Task ManyAsync() => DoTestAsync("Hello\nWorld\nFoo\nBar\nBaz", "Hello","World",
        "Foo", "Bar", "Baz");
}