using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class ShaderOperatorParserTest : ParserTest
{
    [Fact]
    public Task DrawShaderAsync() => TestInputAsync("/DeviceRGB sh", i => i.PaintShaderAsync(KnownNames.DeviceRGB));
}