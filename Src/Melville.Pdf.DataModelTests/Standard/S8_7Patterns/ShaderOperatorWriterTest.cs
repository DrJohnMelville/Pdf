using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class ShaderOperatorWriterTest : WriterTest
{
    [Fact]
    public async Task SetColorSpace()
    {
        await sut.PaintShaderAsync("JdmColor");
        Assert.Equal("/JdmColor sh\n", await WrittenText());
    }
}