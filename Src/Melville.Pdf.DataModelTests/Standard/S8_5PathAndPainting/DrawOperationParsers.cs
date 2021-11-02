 using System;
 using System.Collections.Generic;
 using System.IO.Pipelines;
 using System.Linq.Expressions;
 using System.Threading.Tasks;
 using Melville.INPC;
 using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
 using Melville.Pdf.LowLevel.Model.ContentStreams;
 using Melville.Pdf.LowLevel.Model.Conventions;
 using Melville.Pdf.LowLevel.Model.Objects;
 using Melville.Pdf.LowLevel.Parsing.ContentStreams;
 using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public partial class DrawOperationParsers: ParserTest
{
    [Fact]
    public Task ParseMoveTo() => TestInput(
        "99 23 m", i => i.MoveTo(99,23));
    [Fact]
    public Task ParseLineTo() => TestInput(
        "99 23 l", i => i.LineTo(99,23));
    [Fact]
    public Task ParseCurveTo() => TestInput(
        "1 2 3 4 5 6 c", i => i.CurveTo(1,2,3,4,5,6));
    [Fact]
    public Task ParseCurveToWithoutInitialControl() => TestInput(
        "3 4 5 6 v", i => i.CurveToWithoutInitialControl(3,4,5,6));
    [Fact]
    public Task ParseCurveToWithoutFinalControl() => TestInput(
        "3 4 5 6 y", i => i.CurveToWithoutFinalControl(3,4,5,6));
    [Fact]
    public Task ParseClosePath() => TestInput("h", i => i.ClosePath());
    [Fact]
    public Task ParseRectangle() => TestInput("1 2 3 4 re", i => i.Rectangle(1,2,3,4));
    
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
    public Task EmptyOperator(string code, Expression<Action<IContentStreamOperations>> op) =>
        TestInput(code+"\n", op);

    private partial class StrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetStrokeColor(in ReadOnlySpan<double> color)
        {
            Assert.Equal(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Fact]
    public Task SetColor() => TestInput("1 2 3 SC", new StrokeColorMock());
    
    private partial class NonStrokeColorMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        public void SetNonstrokingColor(in ReadOnlySpan<double> color)
        {
            Assert.Equal(new double[]{1,2,3}, color.ToArray());
            SetCalled();
            
        }
    }

    [Fact]
    public Task SetNonstrokinColor() => TestInput("1 2 3 sc", new NonStrokeColorMock());

    private partial class StrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfName? expectedName;

        public StrokeColorExtendedMock(PdfName? expectedName)
        {
            this.expectedName = expectedName;
        }

        public void SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
        }
    }
    [Fact]
    public Task SetStrokingExtended1() => TestInput("1 2 3 SCN", 
        new StrokeColorExtendedMock(null));
    [Fact]
    public Task SetStrokingExtended2() => TestInput("1 2 3 /P1 SCN", 
        new StrokeColorExtendedMock(NameDirectory.Get("P1")));

    private partial class NonStrokeColorExtendedMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations fake = null!;

        private PdfName? expectedName;

        public NonStrokeColorExtendedMock(PdfName? expectedName)
        {
            this.expectedName = expectedName;
        }

        public void SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
        {
            Assert.Equal(expectedName, patternName);
            Assert.Equal(new double[]{1,2,3}, colors.ToArray());
            SetCalled();
        }
    }
    [Fact]
    public Task SetNonStrokingExtended1() => TestInput("1 2 3 scn", 
        new NonStrokeColorExtendedMock(null));
    [Fact]
    public Task SetNonStrokingExtended2() => TestInput("1 2 3 /P1 scn", 
        new NonStrokeColorExtendedMock(NameDirectory.Get("P1")));
    
    [Fact]
    public Task SetStrokeGray() => TestInput("12 G", i => i.SetStrokeGray(12));
    [Fact]
    public Task SetStrokeRGB() => TestInput("4 5 6 RG", i => i.SetStrokeRGB(4,5, 6));
    [Fact]
    public Task SetStrokeCMYK() => TestInput("4 5 6 7 K", i => i.SetStrokeCMYK(4,5, 6, 7));
    [Fact]
    public Task SetNonstrokingGray() => TestInput("12 g", i => i.SetNonstrokingGray(12));
    [Fact]
    public Task SetNonstrokingRGB() => TestInput("4 5 6 rg", i => i.SetNonstrokingRGB(4,5, 6));
    [Fact]
    public Task SetNonstrokingCMYK() => TestInput("4 5 6 7 k", i => i.SetNonstrokingCMYK(4,5, 6, 7));

}