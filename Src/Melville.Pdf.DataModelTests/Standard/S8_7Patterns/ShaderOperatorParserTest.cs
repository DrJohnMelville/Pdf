using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class ShaderOperatorParserTest : ParserTest
{
    [Fact]
    public Task DrawShader() => TestInput("/DeviceRGB sh", i => i.PaintShader(KnownNames.DeviceRGB));
}