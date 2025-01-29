using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateWriters:WriterTest
{
    [Fact]
    public async Task PushGraphicStateAsync()
    {
        sut.SaveGraphicsState();
        Assert.Equal("q\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task RestoreGraphicsStateAsync()
    {
        sut.RestoreGraphicsState();
        Assert.Equal("Q\n", await WrittenTextAsync());
        
    }
    [Fact]
    public async Task CompositePushAsync()
    {
        sut.SaveGraphicsState();
        sut.RestoreGraphicsState();
        Assert.Equal("q\nQ\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task ModifyTransformMatrixAsync()
    {
        sut.ModifyTransformMatrix(new Matrix3x2(1,2.45f,3,4,-500.1234f,6));
        Assert.Equal("1 2.450000047683716 3 4 -500.1234130859375 6 cm\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetLineWidthAsync()
    {
        sut.SetLineWidth(43);
        Assert.Equal("43 w\n", await WrittenTextAsync());
    }
    [Theory]
    [InlineData(LineCap.Butt, 0)]
    [InlineData(LineCap.Round,1)]
    [InlineData(LineCap.Square,2 )]
    public async Task SetLineCapAsync(LineCap cap, int num)
    {
        sut.SetLineCap(cap);
        Assert.Equal($"{num} J\n", await WrittenTextAsync());
    }
    [Theory]
    [InlineData(LineJoinStyle.Miter, 0)]
    [InlineData(LineJoinStyle.Round, 1)]
    [InlineData(LineJoinStyle.Bevel, 2)]
    public async Task SetLineJoinStyleAsync(LineJoinStyle cap, int num)
    {
        sut.SetLineJoinStyle(cap);
        Assert.Equal($"{num} j\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetMiterLimitAsync()
    {
        sut.SetMiterLimit(1.4223);
        Assert.Equal("1.4223 M\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetLineDashPatternAsync()
    {
        sut.SetLineDashPattern(6, new double[]{1,2,3,4,5});
        Assert.Equal("[1 2 3 4 5] 6 d\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetLineDashPattern0Async()
    {
        sut.SetLineDashPattern(0, new double[]{});
        Assert.Equal("[] 0 d\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetLineDashPattern1Async()
    {
        sut.SetLineDashPattern(0, new double[]{0.54});
        Assert.Equal("[0.54] 0 d\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetLineDashPattern2Async()
    {
        sut.SetLineDashPattern(0, new double[]{0.54, 10});
        Assert.Equal("[0.54 10] 0 d\n", await WrittenTextAsync());
    }
    [Fact]
    public async Task SetRenderingIntentAsync()
    {
        sut.SetRenderIntent(RenderIntentName.Perceptual);
        Assert.Equal("/Perceptual ri\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task SetFlatnessTolerenceAsync()
    {
        sut.SetFlatnessTolerance(52);
        Assert.Equal("52 i\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task LoadGraphicStateDictionaryAsync()
    {
        await sut.LoadGraphicStateDictionaryAsync("JdmGState");
        Assert.Equal("/JdmGState gs\n", await WrittenTextAsync());
    }
}