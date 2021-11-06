using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Primitives;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_8ContentStreams;

public class CompatibilitySectionParserTest: ParserTest
{
    [Fact]
    public Task FailParseWithoutCompatibility() =>
        Assert.ThrowsAsync<PdfParseException>(
            () => TestInput("123 (hello) JDM", i => i.EndPathWithNoOp()));
    [Fact]
    public Task IgnoreUnknownOperatorInCompatibilitySection() =>
            TestInput("BX\n123 (hello) JDM\nEX", 
                i => i.BeginCompatibilitySection(),
                i=>i.EndCompatibilitySection());
}