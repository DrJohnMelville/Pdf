using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class GraphicStateWriters:WriterTest
{
    [Fact]
    public async Task PushGraphicState()
    {
        sut.SaveGraphicsState();
        Assert.Equal("q\n", await WrittenText());
    }

    [Fact]
    public async Task RestoreGraphicsState()
    {
        sut.RestoreGraphicsState();
        Assert.Equal("Q\n", await WrittenText());
        
    }
    [Fact]
    public async Task CompositePush()
    {
        sut.SaveGraphicsState();
        sut.RestoreGraphicsState();
        Assert.Equal("q\nQ\n", await WrittenText());
    }
    [Fact]
    public async Task ModifyTransformMatrix()
    {
        sut.ModifyTransformMatrix(new Matrix3x2(1,2.45f,3,4,-500.1234f,6));
        Assert.Equal("1 2.450000047683716 3 4 -500.123413085938 6 cm\n", await WrittenText());
    }
    [Fact]
    public async Task SetLineWidth()
    {
        sut.SetLineWidth(43);
        Assert.Equal("43 w\n", await WrittenText());
    }
    [Theory]
    [InlineData(LineCap.Butt, 0)]
    [InlineData(LineCap.Round,1)]
    [InlineData(LineCap.Square,2 )]
    public async Task SetLineCap(LineCap cap, int num)
    {
        sut.SetLineCap(cap);
        Assert.Equal($"{num} J\n", await WrittenText());
    }
    [Theory]
    [InlineData(LineJoinStyle.Miter, 0)]
    [InlineData(LineJoinStyle.Round, 1)]
    [InlineData(LineJoinStyle.Bevel, 2)]
    public async Task SetLineJoinStyle(LineJoinStyle cap, int num)
    {
        sut.SetLineJoinStyle(cap);
        Assert.Equal($"{num} j\n", await WrittenText());
    }
    [Fact]
    public async Task SetMiterLimit()
    {
        sut.SetMiterLimit(1.4223);
        Assert.Equal("1.4223 M\n", await WrittenText());
    }
    [Fact]
    public async Task SetLineDashPattern()
    {
        sut.SetLineDashPattern(6, new double[]{1,2,3,4,5});
        Assert.Equal("[1 2 3 4 5] 6 d\n", await WrittenText());
    }
    [Fact]
    public async Task SetLineDashPattern0()
    {
        sut.SetLineDashPattern(0, new double[]{});
        Assert.Equal("[] 0 d\n", await WrittenText());
    }
    [Fact]
    public async Task SetLineDashPattern1()
    {
        sut.SetLineDashPattern(0, new double[]{0.54});
        Assert.Equal("[0.54] 0 d\n", await WrittenText());
    }
    [Fact]
    public async Task SetLineDashPattern2()
    {
        sut.SetLineDashPattern(0, new double[]{0.54, 10});
        Assert.Equal("[0.54 10] 0 d\n", await WrittenText());
    }
    [Fact]
    public async Task SetRenderingIntent()
    {
        sut.SetRenderIntent(RenderIntentName.Perceptual);
        Assert.Equal("/Perceptual ri\n", await WrittenText());
    }

    [Fact]
    public async Task SetFlatnessTolerence()
    {
        sut.SetFlatnessTolerance(52);
        Assert.Equal("52 i\n", await WrittenText());
    }

    [Fact]
    public async Task LoadGraphicStateDictionary()
    {
        await sut.LoadGraphicStateDictionary("JdmGState");
        Assert.Equal("/JdmGState gs\n", await WrittenText());
    }
}