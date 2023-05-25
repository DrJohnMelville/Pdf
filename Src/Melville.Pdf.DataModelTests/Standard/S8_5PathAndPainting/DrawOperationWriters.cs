using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class DrawOperationWriters : WriterTest
{
    [Fact]
    public async Task MoveToAsync()
    {
        sut.MoveTo(21, 34);
        Assert.Equal("21 34 m\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task LineToAsync()
    {
        sut.LineTo(21, 34);
        Assert.Equal("21 34 l\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task CurveToAsync()
    {
        sut.CurveTo(1, 2, 3, 4, 5, 6);
        Assert.Equal("1 2 3 4 5 6 c\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task CurveToWithoutInitialControlAsync()
    {
        sut.CurveToWithoutInitialControl(1, 2, 5, 6);
        Assert.Equal("1 2 5 6 v\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task CurveToWithoutFinalControlAsync()
    {
        sut.CurveToWithoutFinalControl(1, 2, 5, 6);
        Assert.Equal("1 2 5 6 y\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task ClosePathAsync()
    {
        sut.ClosePath();
        Assert.Equal("h\n", await WrittenTextAsync());
    }

    [Fact]
    public async Task RectangleAsync()
    {
        sut.Rectangle(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 re\n", await WrittenTextAsync());
    }

    public static object[] PaintingOperator(
        string code, Action<IContentStreamOperations> op) =>
        new object[] { code, op };

    public static IEnumerable<object[]> PaintinOperators() =>
        new[]
        {
            PaintingOperator("S", i => i.StrokePath()),
            PaintingOperator("s", i => i.CloseAndStrokePath()),
            PaintingOperator("f", i => i.FillPath()),
            PaintingOperator("f*", i => i.FillPathEvenOdd()),
            PaintingOperator("B", i => i.FillAndStrokePath()),
            PaintingOperator("B*", i => i.FillAndStrokePathEvenOdd()),
            PaintingOperator("b", i => i.CloseFillAndStrokePath()),
            PaintingOperator("b*", i => i.CloseFillAndStrokePathEvenOdd()),
            PaintingOperator("n", i=>i.EndPathWithNoOp()),
            PaintingOperator("W", i=>i.ClipToPath()),
            PaintingOperator("W*", i=>i.ClipToPathEvenOdd()),
        };

    [Theory]
    [MemberData(nameof(PaintinOperators))]
    public async Task EmptyOperatorAsync(string code, Action<IContentStreamOperations> op)
    {
        op(sut);
        Assert.Equal(code+"\n", await WrittenTextAsync());
        
    }

    [Fact]
    public async Task nameAsync()
    {
        await sut.DoAsync("N1");
        Assert.Equal("/N1 Do\n", await WrittenTextAsync());
        
    }
}