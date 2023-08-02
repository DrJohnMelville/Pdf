using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class DrawingMacroOperations
{
    private readonly Mock<IRenderTarget> target = new();
    private readonly Mock<IDrawTarget> drawTarget = new();
    private readonly RenderEngine sut;

    public DrawingMacroOperations()
    {
        target.Setup(i => i.CreateDrawTarget()).Returns(drawTarget.Object);
        var page = new PdfPage(new ValueDictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, new(target.Object, 
            DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance),
            NullOptionalContentCounter.Instance));
    }

    [Fact]
    public void Rectangle()
    {
        sut.Rectangle(7,13,17, 19);
        drawTarget.Verify(i=>i.MoveTo(7,13), Times.Once);
        drawTarget.Verify(i=>i.LineTo(24,13), Times.Once);
        drawTarget.Verify(i=>i.LineTo(24,32), Times.Once);
        drawTarget.Verify(i=>i.LineTo(7,32), Times.Once);
        drawTarget.Verify(i=>i.ClosePath(), Times.Once);
        drawTarget.VerifyNoOtherCalls();
    }

    [Fact]
    public void StrokePath()
    {
        DrawNontrivialLine();
        sut.StrokePath();
        VerifyPathOperation(true, false, false, false);
    }

    private void DrawNontrivialLine()
    {
        sut.MoveTo(10, 10);
        sut.LineTo(10, 12);
    }

    [Fact]
    public void CloseAndStrokePath()
    {
        DrawNontrivialLine();
        sut.CloseAndStrokePath();
        VerifyPathOperation(true, false, false, true);
    }
    [Fact]
    public void FillPath()
    {
        DrawNontrivialLine();
        sut.FillPath();
        VerifyPathOperation(false, true, false, false);
    }
    [Fact]
    public void FillEvenOdd()
    {
        DrawNontrivialLine();
        sut.FillPathEvenOdd();
        VerifyPathOperation(false, true, true, false);
    }
    [Fact]
    public void FillAndStrokePath()
    {
        DrawNontrivialLine();
        sut.FillAndStrokePath();
        VerifyPathOperation(true, true, false, false);
    }
    [Fact]
    public void FillAndStrokePathEvenOdd()
    {
        DrawNontrivialLine();
        sut.FillAndStrokePathEvenOdd();
        VerifyPathOperation(true, true, true, false);
    }
    [Fact]
    public void CloseFillAndStrokePath()
    {
        DrawNontrivialLine();
        sut.CloseFillAndStrokePath();
        VerifyPathOperation(true, true, false, true);
    }
    [Fact]
    public void CloseFillAndStrokePathEvenOdd()
    {
        DrawNontrivialLine();
        sut.CloseFillAndStrokePathEvenOdd();
        VerifyPathOperation(true, true, true, true);
    }

    private void VerifyPathOperation(bool stroke, bool fill, bool evenOddFillRule, bool closePath)
    {
        if (closePath) drawTarget.Verify(i=>i.ClosePath());
        drawTarget.Verify(i=>i.MoveTo(10,10));
        drawTarget.Verify(i=>i.LineTo(10,12));
        drawTarget.Verify(i => i.PaintPath(stroke, fill, evenOddFillRule));
        drawTarget.Verify(i=>i.Dispose());
        drawTarget.VerifyNoOtherCalls();
        target.Verify(i=>i.CreateDrawTarget(), Times.Once);
        target.VerifyGet(i=>i.GraphicsState, Times.AtLeast(1));
        target.VerifyNoOtherCalls();
    }
}