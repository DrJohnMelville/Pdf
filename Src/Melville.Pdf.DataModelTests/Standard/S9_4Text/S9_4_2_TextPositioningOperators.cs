using System;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class S9_4_2_TextPositioningOperators
{
    private readonly GraphicsStateStack state = new();
    private readonly Mock<IHasPageAttributes> pageMock = new(MockBehavior.Strict);
    private readonly Mock<IRenderTarget> targetMock = new(MockBehavior.Strict);
    private readonly RenderEngine sut;

    public S9_4_2_TextPositioningOperators()
    {
        targetMock.SetupGet(i => i.GrapicsStateChange).Returns(state);
        sut = new RenderEngine(pageMock.Object, targetMock.Object, new FontReader(new WindowsDefaultFonts()));
    }

    [Fact]
    public void TranslateTextPosition()
    {
        VerifyBothTextMatrices(Matrix3x2.Identity);
        sut.MovePositionBy(10, 20);
        VerifyBothTextMatrices(new Matrix3x2(1, 0, 0, 1, 10, 20));
        sut.MovePositionBy(7, 9);
        VerifyBothTextMatrices(new Matrix3x2(1, 0, 0, 1, 17, 29));
    }

    [Fact]
    public void SetTextMatrixTest()
    {
        sut.SetTextMatrix(1, 2, 3, 4, 5, 6);
        VerifyBothTextMatrices(new Matrix3x2(1, 2, 3, 4, 5, 6));
    }

    [Fact]
    public void BeginTextObjectResetsTextMatrices()
    {
        sut.MovePositionBy(10, 20);
        VerifyBothTextMatrices(new Matrix3x2(1, 0, 0, 1, 10, 20));
        sut.BeginTextObject();
        VerifyBothTextMatrices(Matrix3x2.Identity);
    }

    private void VerifyBothTextMatrices(Matrix3x2 expected)
    {
        Assert.Equal(expected, state.Current().TextMatrix);
        Assert.Equal(expected, state.Current().TextLineMatrix);
    }

    [Fact]
    public void TranslateTextPositionWithLeading()
    {
        sut.MovePositionByWithLeading(10, -20);
        VerifyBothTextMatrices(new Matrix3x2(1, 0, 0, 1, 10, -20));
        Assert.Equal(20, sut.CurrentState().TextLeading);
    }

    [Fact]
    public void MoveToNextTextLine()
    {
        sut.SetTextLeading(20);
        sut.MoveToNextTextLine();
        VerifyBothTextMatrices(new Matrix3x2(1, 0, 0, 1, 0, -20));
    }

    [Theory]
    [InlineData("e", 10)]
    [InlineData("ee", 20)]
    public void DrawString(string input, float xPos)
    {
//        targetMock.Setup(i => i.AddGlyphToCurrentString((byte)'e')).Returns((10.0, 12.0));
 //       targetMock.Setup(i => i.RenderCurrentString());
        sut.ShowString(input.AsExtendedAsciiBytes());
        Assert.Equal(Matrix3x2.Identity, sut.CurrentState().TextLineMatrix);
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, xPos, 0), sut.CurrentState().TextMatrix);
    }

    [Fact]
    public void DrawHorizontalCompressedStream()
    {
   //     targetMock.Setup(i => i.AddGlyphToCurrentString((byte)' ')).Returns((10.0, 12.0));
     //   targetMock.Setup(i => i.RenderCurrentString());
        sut.SetCharSpace(20);
        sut.SetWordSpace(30);
        sut.SetHorizontalTextScaling(50);
        sut.ShowString(" ".AsExtendedAsciiBytes());
        Assert.Equal(new Matrix3x2(1,0,0,1,30, 0), sut.CurrentState().TextMatrix);
        
    }

    [Fact]
    public void MoveToNextTextLineAndShowString()
    {
//        targetMock.Setup(i => i.AddGlyphToCurrentString(65)).Returns((10.0, 12.0));
 //       targetMock.Setup(i => i.RenderCurrentString());
        sut.SetTextLeading(20);
        sut.MoveToNextLineAndShowString(new ReadOnlyMemory<byte>(new byte[] { 65 }));
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, 10, -20), sut.CurrentState().TextMatrix);
    }
    
    [Fact]
    public void MoveToNextTextLineAndShowStringWithSpacing()
    {
   //     targetMock.Setup(i => i.AddGlyphToCurrentString(65)).Returns((10.0, 12.0));
     //   targetMock.Setup(i => i.RenderCurrentString());
        sut.SetTextLeading(20);
        sut.MoveToNextLineAndShowString(4,5,new ReadOnlyMemory<byte>(new byte[] { 65 }));
        Assert.Equal(4.0, sut.CurrentState().WordSpacing);
        Assert.Equal(5.0, sut.CurrentState().CharacterSpacing);
        
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, 15, -20), sut.CurrentState().TextMatrix);
    }

    [Theory]
    [InlineData(100,9)]
    [InlineData(50,4.5)]
    [InlineData(1000,90)]
    public void ShowSpacedStream(double horizontalScale, float xPosition)
    {
//        targetMock.Setup(i => i.AddGlyphToCurrentString((byte)'e')).Returns((10.0, 12.0));
  //      targetMock.Setup(i => i.RenderCurrentString());
        sut.SetHorizontalTextScaling(horizontalScale);
        sut.ShowSpacedString(
            new []
            {
                new ContentStreamValueUnion("e".AsExtendedAsciiBytes()),
                new ContentStreamValueUnion(1000, 1000)
            });
        Assert.Equal(Matrix3x2.Identity,
            sut.CurrentState().TextLineMatrix);
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, xPosition, 0), sut.CurrentState().TextMatrix);
    }
}