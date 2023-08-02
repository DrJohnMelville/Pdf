 using System;
 using System.Collections.Generic;
 using System.Linq.Expressions;
 using System.Threading.Tasks;
 using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
 using Melville.Pdf.LowLevel.Model.ContentStreams;
 using Melville.Pdf.LowLevel.Model.Objects;
 using Xunit;
 
 namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class DrawOperationParsers: ParserTest
{
    [Fact]
    public Task ParseMoveToAsync() => TestInputAsync(
        "99 23 m", i => i.MoveTo(99,23));
    [Fact]
    public Task ParseLineToAsync() => TestInputAsync(
        "99 23 l", i => i.LineTo(99,23));
    [Fact]
    public Task ParseCurveToAsync() => TestInputAsync(
        "1 2 3 4 5 6 c", i => i.CurveTo(1,2,3,4,5,6));
    [Fact]
    public Task ParseCurveToWithoutInitialControlAsync() => TestInputAsync(
        "3 4 5 6 v", i => i.CurveToWithoutInitialControl(3,4,5,6));
    [Fact]
    public Task ParseCurveToWithoutFinalControlAsync() => TestInputAsync(
        "3 4 5 6 y", i => i.CurveToWithoutFinalControl(3,4,5,6));
    [Fact]
    public Task ParseClosePathAsync() => TestInputAsync("h", i => i.ClosePath());
    [Fact]
    public Task ParseRectangleAsync() => TestInputAsync("1 2 3 4 re", i => i.Rectangle(1,2,3,4));
    
    public static object[] PaintingOperator(
        string code, Expression<Action<IContentStreamOperations>> op) =>
        new object[] { code, op };

    public static IEnumerable<object[]> PaintinOperators() =>
        new[]
        {
            PaintingOperator("S", i => i.StrokePath()),
            PaintingOperator("s", i => i.CloseAndStrokePath()),
            PaintingOperator("f", i => i.FillPath()),
            PaintingOperator("F", i => i.FillPath()),
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
    public Task EmptyOperatorAsync(string code, Expression<Action<IContentStreamOperations>> op) =>
        TestInputAsync(code, op);

    [Fact]
    public Task DoOperationTestAsync()
    {
        var name = PdfDirectValue.CreateName("BBB");
        return TestInputAsync("/BBB Do", i=>i.DoAsync(name));
        
    }
}