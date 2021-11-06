using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_8ContentStreams;

public class CompatibilitySectionWriterTest:WriterTest
{
    // [Fact]
    // public async Task CompatibilitySection()
    // {
    //     using (sut.BeginCompatibilitySection("M2"))
    //     {
    //         sut.MarkedContentPoint("M1");
    //     }
    //     Assert.Equal("/M2 BMC\n/M1 MP\nEMC\n", await WrittenText());
    // }
}