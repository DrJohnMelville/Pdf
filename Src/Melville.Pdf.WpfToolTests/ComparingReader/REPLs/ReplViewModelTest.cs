using System;
using System.Threading.Tasks;
using Melville.Pdf.ComparingReader.Renderers;
using Melville.Pdf.ComparingReader.REPLs;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.Wpf.Controls;
using Moq;
using Xunit;

namespace Melville.Pdf.WpfToolTests.ComparingReader.REPLs;

public class ReplViewModelTest
{
    [Fact]
    public async Task ReplViewModelCanPrettyPrintAsync()
    {
        var sut = new ReplViewModel("q Q",
            Mock.Of<IMultiRenderer>(), 
            Array.Empty<byte>(),
            ((PdfDirectValue)new ValueDictionaryBuilder().AsStream("q Q")),
                Mock.Of<IPageSelector>());
        await sut.PrettyPrintAsync();
        Assert.Equal("q\nQ\n", sut.ContentStreamText);
    }

    [Theory]
    [InlineData("q 10 Ts Q", "q\n    10 Ts\nQ\n")]
    [InlineData("q q 10 Ts Q Q", "q\n    q\n        10 Ts\n    Q\nQ\n")]
    [InlineData("BT 10 Ts ET", "BT\n    10 Ts\nET\n")]
    public async Task PrintIndentedContentStreamAsync(string source, string dest)
    {
        Assert.Equal(dest, await ContentStreamPrettyPrinter.PrettyPrintAsync(source));
        
    }
}