﻿using System;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_5PathAndPainting;

public class TrivialPathDetectionTest
{
    private readonly Mock<IDrawTarget> drawTarget = new();
    private readonly Mock<IGraphicsState> graphicsState = new();
    private readonly PathDrawingAdapter sut;

    public TrivialPathDetectionTest()
    {
        sut = new PathDrawingAdapter(graphicsState.Object, drawTarget.Object);
    }
    private void SetLineCapMode(LineCap cap)
    {
        var graphicStatMock = new TestGraphicsState();
        graphicStatMock.SetLineCap(cap);
        graphicsState.Setup(i => i.CurrentState()).Returns(graphicStatMock);
    }

    [Fact]
    public void BlockTrivialPathFills()
    {
        sut.MoveTo(10, 10);
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Never);
    }

    private void VerifyTotalDraw(Func<Times> times) => VerifyTotalDraw(times());
    private void VerifyTotalDraw(Times times)
    {
        drawTarget.Verify(i => i.PaintPath(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), times);
    }

    [Fact]
    public void DrawNonTrivialLine()
    {
        sut.MoveTo(10, 10);
        sut.LineTo(10, 20);
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Once);
    }
    [Fact]
    public void DrawQuadraticCurve()
    {
        sut.MoveTo(10, 10);
        sut.CurveTo(1,2,3,4,5,6);
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Once);
    }

    [Theory]
    [InlineData(LineCap.Round, 1)]
    [InlineData(LineCap.Butt, 0)]
    [InlineData(LineCap.Square, 0)]
    public void TrivialLineDraw(LineCap cap, int numberOfDraws)
    {
        SetLineCapMode(cap);
        sut.MoveTo(10,10);
        sut.LineTo(10,10);
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Exactly(numberOfDraws));
    }
    [Theory]
    [InlineData(LineCap.Round, 1)]
    [InlineData(LineCap.Butt, 0)]
    [InlineData(LineCap.Square, 0)]
    public void TrivialClosedPathDraw(LineCap cap, int numberOfDraws)
    {
        SetLineCapMode(cap);
        sut.MoveTo(10,10);
        sut.ClosePath();
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Exactly(numberOfDraws));
    }
    [Theory]
    [InlineData(LineCap.Round)]
    [InlineData(LineCap.Butt)]
    [InlineData(LineCap.Square)]
    public void TrivialDrawAtEndDoesNotCancelPainting(LineCap cap)
    {
        SetLineCapMode(cap);
        sut.MoveTo(10,10);
        sut.LineTo(20,10);
        sut.LineTo(10,10);
        sut.LineTo(10,10);
        sut.FillAndStrokePathEvenOdd();;
        VerifyTotalDraw(Times.Once);
    }
}