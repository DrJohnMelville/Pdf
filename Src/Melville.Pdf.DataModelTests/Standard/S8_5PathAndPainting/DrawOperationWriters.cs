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
    public async Task MoveTo()
    {
        sut.MoveTo(21, 34);
        Assert.Equal("21 34 m\n", await WrittenText());
    }

    [Fact]
    public async Task LineTo()
    {
        sut.LineTo(21, 34);
        Assert.Equal("21 34 l\n", await WrittenText());
    }

    [Fact]
    public async Task CurveTo()
    {
        sut.CurveTo(1, 2, 3, 4, 5, 6);
        Assert.Equal("1 2 3 4 5 6 c\n", await WrittenText());
    }

    [Fact]
    public async Task CurveToWithoutInitialControl()
    {
        sut.CurveToWithoutInitialControl(1, 2, 5, 6);
        Assert.Equal("1 2 5 6 v\n", await WrittenText());
    }

    [Fact]
    public async Task CurveToWithoutFinalControl()
    {
        sut.CurveToWithoutFinalControl(1, 2, 5, 6);
        Assert.Equal("1 2 5 6 y\n", await WrittenText());
    }

    [Fact]
    public async Task ClosePath()
    {
        sut.ClosePath();
        Assert.Equal("h\n", await WrittenText());
    }

    [Fact]
    public async Task Rectangle()
    {
        sut.Rectangle(1, 2, 3, 4);
        Assert.Equal("1 2 3 4 re\n", await WrittenText());
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
    public async Task EmptyOperator(string code, Action<IContentStreamOperations> op)
    {
        op(sut);
        Assert.Equal(code+"\n", await WrittenText());
        
    }

    [Fact]
    public async Task name()
    {
        sut.Do("N1");
        Assert.Equal("/N1 Do\n", await WrittenText());
        
    }
}