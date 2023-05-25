using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_8ContentStreams;

public class CompatibilitySectionParserTest: ParserTest
{
    [Theory]
    [InlineData("123 (hello) JDM")]
    [InlineData("BX EX 123 (hello) JDM")]
    [InlineData("BX EX JDM BX EX")]
    public Task FailParseWithoutCompatibilityAsync(string illeal) =>
        Assert.ThrowsAsync<PdfParseException>(
            () => TestInputAsync(illeal, i => i.EndPathWithNoOp()));

    [Fact]
    public Task IgnoreUnknownOperatorInCompatibilitySectionAsync() =>
            TestInputAsync("BX\n123 (hello) JDM\nEX", 
                i => i.BeginCompatibilitySection(),
                i=>i.EndCompatibilitySection());
    [Fact]
    public Task IgnoreUnknownOperatorInCompatibilitySection2Async() =>
            TestInputAsync("BX BX EX\n123 (hello) JDM\nEX", 
                i => i.BeginCompatibilitySection(),
                i => i.BeginCompatibilitySection(),
                i=>i.EndCompatibilitySection(),
                i=>i.EndCompatibilitySection());
}