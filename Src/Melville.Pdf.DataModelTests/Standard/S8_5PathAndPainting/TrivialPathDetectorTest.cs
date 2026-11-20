using System.Security.Principal;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Moq;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class TrivialPathDetectorTest
{
    private readonly Mock<IGraphicsState> gs = new();
    private readonly Mock<IDrawTarget> target = new();
    private readonly GraphicsState gState = new TestGraphicsState();
    private readonly PathDrawingAdapter sut;

    public TrivialPathDetectorTest()
    {
        gs.Setup(i => i.CurrentState()).Returns(gState);
        sut = new PathDrawingAdapter(gs.Object, target.Object);
    }

    [Test]
    public void NoMoveToIsTrivial()
    {
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Never);
    }

    [Test]
    public void JustMoveToIsTrivial()
    {
        sut.MoveTo(10, 23);
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Never);
    }

    [Test]
    public void MoveToWithIdenticalLineToIsTrivial()
    {
        sut.MoveTo(10, 23);
        sut.LineTo(10, 23);
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Never);
    }

    [Test]
    public void IdenticalLineWithRoundCapsToIsNotTrivial()
    {
        gState.SetLineCap(LineCap.Round);
        sut.MoveTo(10, 23);
        sut.LineTo(10, 23);
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Once);
    }

    [Test]
    public void LineWithLengthIsNotTrivial()
    {
        sut.MoveTo(10, 23);
        sut.LineTo(10, 231);
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Once);
    }

    [Test]
    public void SpecificPaintingBug()
    {
        sut.MoveTo(0,0);
        sut.LineTo(0, 0);
        sut.LineTo(10, 0);
        sut.StrokePath();
        target.Verify(i=>i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(),Equals(It.IsAny<bool>())),
            Times.Once);
    }
}