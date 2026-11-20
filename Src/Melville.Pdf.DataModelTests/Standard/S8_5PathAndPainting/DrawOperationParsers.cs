 using System;
 using System.Collections.Generic;
 using System.Linq.Expressions;
 using System.Threading.Tasks;
 using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
 using Melville.Pdf.LowLevel.Model.ContentStreams;
 using Melville.Pdf.LowLevel.Model.Objects;
  
 namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class DrawOperationParsers: ParserTest
{
    [Test]
    public Task ParseMoveToAsync() => TestInputAsync(
        "99 23 m", i => i.MoveTo(99,23));
    [Test]
    public Task ParseLineToAsync() => TestInputAsync(
        "99 23 l", i => i.LineTo(99,23));
    [Test]
    public Task ParseCurveToAsync() => TestInputAsync(
        "1 2 3 4 5 6 c", i => i.CurveTo(1,2,3,4,5,6));
    [Test]
    public Task ParseCurveToWithoutInitialControlAsync() => TestInputAsync(
        "3 4 5 6 v", i => i.CurveToWithoutInitialControl(3,4,5,6));
    [Test]
    public Task ParseCurveToWithoutFinalControlAsync() => TestInputAsync(
        "3 4 5 6 y", i => i.CurveToWithoutFinalControl(3,4,5,6));
    [Test]
    public Task ParseClosePathAsync() => TestInputAsync("h", i => i.ClosePath());
    [Test]
    public Task ParseRectangleAsync() => TestInputAsync("1 2 3 4 re", i => i.Rectangle(1,2,3,4));
    
    public static (string, Expression<Action<IContentStreamOperations>>) PaintingOperator(
        string code, Expression<Action<IContentStreamOperations>> op) =>
        (code, op );

    public static IEnumerable<(string, Expression<Action<IContentStreamOperations>> )> PaintinOperators() =>
        [
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
        ];
    [Test]
    [MethodDataSource(nameof(PaintinOperators))]
    public Task EmptyOperatorAsync(string code, Expression<Action<IContentStreamOperations>> op) =>
        TestInputAsync(code, op);

    [Test]
    public Task DoOperationTestAsync()
    {
        var name = PdfDirectObject.CreateName("BBB");
        return TestInputAsync("/BBB Do", i=>i.DoAsync(name));
        
    }
}