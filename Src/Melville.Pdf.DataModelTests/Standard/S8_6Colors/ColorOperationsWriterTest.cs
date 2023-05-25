using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class ColorOperationsWriterTest: WriterTest
{
    [Fact]
    public async Task SetColorSpaceAsync()
    {
        await sut.SetStrokingColorSpaceAsync("JdmColor");
        Assert.Equal("/JdmColor CS\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonStrokingColorSpaceAsync()
    {
        await sut.SetNonstrokingColorSpaceAsync("JdmColor");
        Assert.Equal("/JdmColor cs\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetStrokeColor1Async()
    {
        sut.SetStrokeColor(1);
        Assert.Equal("1 SC\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetStrokeColor4Async()
    {
        sut.SetStrokeColor(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 SC\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetStrokeColor1ExtendedAsync()
    {
        await sut.SetStrokeColorExtendedAsync(1);
        Assert.Equal("1 SCN\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetStrokeColor4ExtendedAsync()
    {
        await sut.SetStrokeColorExtendedAsync(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 SCN\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetStrokeColor1ExtendedPatternAsync()
    {
        await sut.SetStrokeColorExtendedAsync("pat", 1);
        Assert.Equal("1 /pat SCN\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetStrokeColor4ExtendedPatternAsync()
    {
        await sut.SetStrokeColorExtendedAsync("pat", 1, 2, 3, 4);
        Assert.Equal("1 2 3 4 /pat SCN\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetNonstrokingColor1Async()
    {
        sut.SetNonstrokingColor(1);
        Assert.Equal("1 sc\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonstrokingColor4Async()
    {
        sut.SetNonstrokingColor(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 sc\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonstrokingColor1ExtendedAsync()
    {
        await sut.SetNonstrokingColorExtendedAsync(1);
        Assert.Equal("1 scn\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonstrokingColor4ExtendedAsync()
    {
        await sut.SetNonstrokingColorExtendedAsync(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 scn\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonstrokingColor1ExtendedPatternAsync()
    {
        await sut.SetNonstrokingColorExtendedAsync("pat", 1);
        Assert.Equal("1 /pat scn\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetNonstrokingColor4ExtendedPatternAsync()
    {
        await sut.SetNonstrokingColorExtendedAsync("pat", 1, 2, 3, 4);
        Assert.Equal("1 2 3 4 /pat scn\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetStrokingGrayAsync()
    {
        await sut.SetStrokeGrayAsync(12);
        Assert.Equal("12 G\n", await WrittenTextAsync());
    }    
    
    [Fact]
    public async Task SetStrokingRGBAsync()
    {
        await sut.SetStrokeRGBAsync(1,2,3);
        Assert.Equal("1 2 3 RG\n", await WrittenTextAsync());
    }
 
    [Fact]
    public async Task SetStrokingCMYKAsync()
    {
        await sut.SetStrokeCMYKAsync(1,2,3,4);
        Assert.Equal("1 2 3 4 K\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetNonStrokingGrayAsync()
    {
        await sut.SetNonstrokingGrayAsync(12);
        Assert.Equal("12 g\n", await WrittenTextAsync());
    }    
    
    [Fact]
    public async Task SetNonStrokingRGBAsync()
    {
        await sut.SetNonstrokingRgbAsync(1,2,3);
        Assert.Equal("1 2 3 rg\n", await WrittenTextAsync());
    }
 
    [Fact]
    public async Task SetNonStrokingCMYKAsync()
    {
        await sut.SetNonstrokingCMYKAsync(1,2,3,4);
        Assert.Equal("1 2 3 4 k\n", await WrittenTextAsync());
    }
}