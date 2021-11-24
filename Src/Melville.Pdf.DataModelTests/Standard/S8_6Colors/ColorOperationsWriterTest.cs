using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class ColorOperationsWriterTest: WriterTest
{
    [Fact]
    public async Task SetColorSpace()
    {
        await sut.SetStrokingColorSpace("JdmColor");
        Assert.Equal("/JdmColor CS\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonStrokingColorSpace()
    {
        await sut.SetNonstrokingColorSpace("JdmColor");
        Assert.Equal("/JdmColor cs\n", await WrittenText());
    }

    [Fact]
    public async Task SetStrokeColor1()
    {
        sut.SetStrokeColor(1);
        Assert.Equal("1 SC\n", await WrittenText());
    }
    [Fact]
    public async Task SetStrokeColor4()
    {
        sut.SetStrokeColor(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 SC\n", await WrittenText());
    }
    [Fact]
    public async Task SetStrokeColor1Extended()
    {
        await sut.SetStrokeColorExtended(1);
        Assert.Equal("1 SCN\n", await WrittenText());
    }
    [Fact]
    public async Task SetStrokeColor4Extended()
    {
        await sut.SetStrokeColorExtended(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 SCN\n", await WrittenText());
    }
    [Fact]
    public async Task SetStrokeColor1ExtendedPattern()
    {
        await sut.SetStrokeColorExtended("pat", 1);
        Assert.Equal("1 /pat SCN\n", await WrittenText());
    }
    [Fact]
    public async Task SetStrokeColor4ExtendedPattern()
    {
        await sut.SetStrokeColorExtended("pat", 1, 2, 3, 4);
        Assert.Equal("1 2 3 4 /pat SCN\n", await WrittenText());
    }

    [Fact]
    public async Task SetNonstrokingColor1()
    {
        sut.SetNonstrokingColor(1);
        Assert.Equal("1 sc\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonstrokingColor4()
    {
        sut.SetNonstrokingColor(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 sc\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonstrokingColor1Extended()
    {
        sut.SetNonstrokingColorExtended(1);
        Assert.Equal("1 scn\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonstrokingColor4Extended()
    {
        sut.SetNonstrokingColorExtended(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 scn\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonstrokingColor1ExtendedPattern()
    {
        sut.SetNonstrokingColorExtended("pat", 1);
        Assert.Equal("1 /pat scn\n", await WrittenText());
    }
    [Fact]
    public async Task SetNonstrokingColor4ExtendedPattern()
    {
        sut.SetNonstrokingColorExtended("pat", 1, 2, 3, 4);
        Assert.Equal("1 2 3 4 /pat scn\n", await WrittenText());
    }

    [Fact]
    public async Task SetStrokingGray()
    {
        sut.SetStrokeGray(12);
        Assert.Equal("12 G\n", await WrittenText());
    }    
    
    [Fact]
    public async Task SetStrokingRGB()
    {
        sut.SetStrokeRGB(1,2,3);
        Assert.Equal("1 2 3 RG\n", await WrittenText());
    }
 
    [Fact]
    public async Task SetStrokingCMYK()
    {
        sut.SetStrokeCMYK(1,2,3,4);
        Assert.Equal("1 2 3 4 K\n", await WrittenText());
    }

    [Fact]
    public async Task SetNonStrokingGray()
    {
        sut.SetNonstrokingGray(12);
        Assert.Equal("12 g\n", await WrittenText());
    }    
    
    [Fact]
    public async Task SetNonStrokingRGB()
    {
        sut.SetNonstrokingRGB(1,2,3);
        Assert.Equal("1 2 3 rg\n", await WrittenText());
    }
 
    [Fact]
    public async Task SetNonStrokingCMYK()
    {
        sut.SetNonstrokingCMYK(1,2,3,4);
        Assert.Equal("1 2 3 4 k\n", await WrittenText());
    }
}