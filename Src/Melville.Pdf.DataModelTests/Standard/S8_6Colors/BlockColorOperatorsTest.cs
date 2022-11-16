using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class BlockColorOperatorsTest: IDisposable
{
    private readonly Mock<IGraphicsState> state = new();
    private readonly Mock<IRenderTarget> target = new();
    private readonly RenderEngine sut;

    public BlockColorOperatorsTest()
    {
        target.SetupGet(i => i.GraphicsState).Returns(state.Object);
        var page = new PdfPage(new DictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, target.Object, 
            DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance),
            NullOptionalContentCounter.Instance);
        sut.SetUncoloredGlyphMetrics(1, 2, 3, 4, 5, 6);
    }

    public void Dispose()
    {
        state.VerifyNoOtherCalls();
        target.VerifyNoOtherCalls();
    }

    [Fact]
    public void BlockCSOperator()
    {
        sut.SetStrokingColorSpace(KnownNames.DeviceRGB);
    }
}