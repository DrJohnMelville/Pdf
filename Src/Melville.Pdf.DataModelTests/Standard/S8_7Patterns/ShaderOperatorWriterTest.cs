using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class ShaderOperatorWriterTest : WriterTest
{
    [Test]
    public async Task SetColorSpaceAsync()
    {
        await sut.PaintShaderAsync("JdmColor");
        Assert.Equal("/JdmColor sh\n", await WrittenTextAsync());
    }
}