using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class DrawingMacroOperations
{
    private readonly Mock<IRenderTarget> target = new();
    private readonly RenderEngine sut;

    public DrawingMacroOperations()
    {
        var page = new PdfPage(new DictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, target.Object);
    }

    [Fact]
    public void Rectangle()
    {
        sut.Rectangle(7,13,17, 19);
        target.Verify(i=>i.MoveTo(7,13), Times.Once);
        target.Verify(i=>i.LineTo(24,13), Times.Once);
        target.Verify(i=>i.LineTo(24,32), Times.Once);
        target.Verify(i=>i.LineTo(7,32), Times.Once);
        target.Verify(i=>i.ClosePath(), Times.Once);
        target.VerifyNoOtherCalls();
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
        if (closePath) target.Verify(i=>i.ClosePath());
        target.Verify(i => i.PaintPath(stroke, fill, evenOddFillRule));
        target.Verify(i => i.EndPath());
        target.VerifyNoOtherCalls();
    }
}