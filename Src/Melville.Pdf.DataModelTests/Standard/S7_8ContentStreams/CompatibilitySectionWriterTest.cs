using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_8ContentStreams;

public class CompatibilitySectionWriterTest:WriterTest
{
    [Fact]
    public async Task CompatibilitySection()
    {
        using (sut.BeginCompatibilitySection())
        {
            sut.MarkedContentPoint("M1");
        }
        Assert.Equal("BX\n/M1 MP\nEX\n", await WrittenText());
    }
}