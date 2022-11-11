using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings;
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
        var page = new PdfPage(new DictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, target.Object, 
            DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance),
            NullOptionalContentCounter.Instance);
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
        sut.StrokePath();
        VerifyPathOperation(true, false, false, false);
    }
    [Fact]
    public void CloseAndStrokePath()
    {
        sut.CloseAndStrokePath();
        VerifyPathOperation(true, false, false, true);
    }
    [Fact]
    public void FillPath()
    {
        sut.FillPath();
        VerifyPathOperation(false, true, false, false);
    }
    [Fact]
    public void FillEvenOdd()
    {
        sut.FillPathEvenOdd();
        VerifyPathOperation(false, true, true, false);
    }
    [Fact]
    public void FillAndStrokePath()
    {
        sut.FillAndStrokePath();
        VerifyPathOperation(true, true, false, false);
    }
    [Fact]
    public void FillAndStrokePathEvenOdd()
    {
        sut.FillAndStrokePathEvenOdd();
        VerifyPathOperation(true, true, true, false);
    }
    [Fact]
    public void CloseFillAndStrokePath()
    {
        sut.CloseFillAndStrokePath();
        VerifyPathOperation(true, true, false, true);
    }
    [Fact]
    public void CloseFillAndStrokePathEvenOdd()
    {
        sut.CloseFillAndStrokePathEvenOdd();
        VerifyPathOperation(true, true, true, true);
    }

    private void VerifyPathOperation(bool stroke, bool fill, bool evenOddFillRule, bool closePath)
    {
        if (closePath) drawTarget.Verify(i=>i.ClosePath());
        drawTarget.Verify(i => i.PaintPath(stroke, fill, evenOddFillRule));
        drawTarget.VerifyNoOtherCalls();
        target.Verify(i=>i.CreateDrawTarget(), Times.Once);
        target.VerifyNoOtherCalls();
    }
}