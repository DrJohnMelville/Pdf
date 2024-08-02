using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public class S9_4_2_TextPositioningOperators
{
    private readonly GraphicsStateStack<TestGraphicsState> state = new();
    private readonly Mock<IHasPageAttributes> pageMock = new(MockBehavior.Strict);
    private readonly Mock<IRenderTarget> targetMock = new(MockBehavior.Strict);
    private readonly RenderEngine sut;
    private readonly IRealizedFont rf;
    private readonly Mock<IFontWriteOperation> fw = new();

    public S9_4_2_TextPositioningOperators()
    {
        rf = new RealizedFontMock(fw.Object);
        targetMock.SetupGet(i => i.GraphicsState).Returns(()=>state.StronglyTypedCurrentState());
        state.CurrentState().SetFontAsync(KnownNames.Courier, 1.0);
        SetupMockRealizedFont();

        sut = new RenderEngine(pageMock.Object, new( targetMock.Object,
            DocumentRendererFactory.CreateRenderer(null!, WindowsDefaultFonts.Instance),
            NullOptionalContentCounter.Instance));
    }
    
    private class RealizedFontMock: IRealizedFont
    {
        private IFontWriteOperation fw;

        public RealizedFontMock(IFontWriteOperation fw)
        {
            this.fw = fw;
        }

        public IReadCharacter ReadCharacter => SingleByteCharacters.Instance;
        public IMapCharacterToGlyph MapCharacterToGlyph => IdentityCharacterToGlyph.Instance;

        public IFontWriteOperation BeginFontWrite(IFontTarget target)
        {
            return fw;
        }

        public IFontWriteOperation BeginFontWriteWithoutTakingMutex(IFontTarget target) => BeginFontWrite(target);

        public double? CharacterWidth(uint character) => default;

        public int GlyphCount => 0;
        public string FamilyName => "";
        public string Description => "";
        public bool IsCachableFont => true;
    }

    private void SetupMockRealizedFont()
    {
        fw.Setup(i => i.AddGlyphToCurrentStringAsync(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<Matrix3x2>()))
            .Returns( ValueTask.CompletedTask);
        fw.Setup(i => i.NativeWidthOfLastGlyphAsync(It.IsAny<uint>())).ReturnsAsync(10.0);  
        fw.Setup(i => i.RenderCurrentString(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<Matrix3x2>()));
        state.StronglyTypedCurrentState().SetTypeface(rf);
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
        Assert.Equal(expected, state.StronglyTypedCurrentState().TextMatrix);
        Assert.Equal(expected, state.StronglyTypedCurrentState().TextLineMatrix);
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
    public async Task DrawStringAsync(string input, float xPos)
    {
        await sut.ShowStringAsync(input.AsExtendedAsciiBytes());
        Assert.Equal(Matrix3x2.Identity, sut.CurrentState().TextLineMatrix);
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, xPos, 0), sut.CurrentState().TextMatrix);
    }

    [Fact]
    public async Task DrawHorizontalCompressedStreamAsync()
    {
        sut.SetCharSpace(20);
        sut.SetWordSpace(30);
        sut.SetHorizontalTextScaling(50);
        await sut.ShowStringAsync(" ".AsExtendedAsciiBytes());
        Assert.Equal(new Matrix3x2(1,0,0,1,30, 0), sut.CurrentState().TextMatrix);
        
    }

    [Fact]
    public void MoveToNextTextLineAndShowString()
    {
        sut.SetTextLeading(20);
        sut.MoveToNextLineAndShowStringAsync(new ReadOnlyMemory<byte>(new byte[] { 65 }));
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, 10, -20), sut.CurrentState().TextMatrix);
    }
    
    [Fact]
    public void MoveToNextTextLineAndShowStringWithSpacing()
    {
        sut.SetTextLeading(20);
        sut.MoveToNextLineAndShowStringAsync(4,5,new ReadOnlyMemory<byte>(new byte[] { 65 }));
        Assert.Equal(4.0, sut.CurrentState().WordSpacing);
        Assert.Equal(5.0, sut.CurrentState().CharacterSpacing);
        
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, 15, -20), sut.CurrentState().TextMatrix);
    }

    [Theory]
    [InlineData(100,9, 1)]
    [InlineData(100,18, 2)]
    [InlineData(50,4.5, 1)]
    [InlineData(1000,90, 1)]
    public async Task ShowSpacedStreamAsync(double horizontalScale, float xPosition, int fontSize)
    {
        sut.SetHorizontalTextScaling(horizontalScale);
        await state.StronglyTypedCurrentState().SetFontAsync(KnownNames.Helvetica, fontSize);
        var builder = sut.GetSpacedStringBuilder();
        await builder.SpacedStringComponentAsync("e".AsExtendedAsciiBytes());
        await builder.SpacedStringComponentAsync(1000);
        await builder.DisposeAsync();
        Assert.Equal(Matrix3x2.Identity,
            sut.CurrentState().TextLineMatrix);
        Assert.Equal(new Matrix3x2(1, 0, 0, 1, xPosition, 0), sut.CurrentState().TextMatrix);
    }
}