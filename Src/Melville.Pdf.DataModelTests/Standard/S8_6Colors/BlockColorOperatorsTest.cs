using System;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

//PDF spec 2.0 clause 8.6.8 dictates color operators which are turned off at certian times
public class BlockColorOperatorsTest: IDisposable
{
    private readonly Mock<IGraphicsState> state = new();
    private readonly Mock<IRenderTarget> target = new();
    private readonly RenderEngine sut;

    public BlockColorOperatorsTest()
    {
        target.SetupGet(i => i.GraphicsState).Returns(state.Object);
        var page = new PdfPage(new DictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, new(target.Object, 
            DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance),
            NullOptionalContentCounter.Instance));
        sut.SetUncoloredGlyphMetrics(1, 2, 3, 4, 5, 6);
        target.VerifyGet(i=>i.GraphicsState, Times.Exactly(2));
    }

    public void Dispose()
    {
        state.VerifyNoOtherCalls();
        target.VerifyNoOtherCalls();
    }

    [Fact] public void BlockCS() => sut.SetStrokingColorSpaceAsync(KnownNames.DeviceRGB);
    [Fact] public void BlockSC() => sut.SetStrokeColor(1, 2, 3);
    [Fact] public void BlockSCN() => sut.SetStrokeColorExtendedAsync(KnownNames.DeviceRGB, 1, 23);
    [Fact] public void BlockG() => sut.SetStrokeGrayAsync(1);
    [Fact] public void BlockRG() => sut.SetStrokeRGBAsync(1,2,3);
    [Fact] public void BlockK() => sut.SetStrokeCMYKAsync(1,2,3, 4);
    [Fact] public void BlockNonstrokinhCS() => sut.SetStrokingColorSpaceAsync(KnownNames.DeviceRGB);
    [Fact] public void BlockNonstrokinhSC() => sut.SetNonstrokingColor(1, 2, 3);
    [Fact] public void BlockNonstrokinhSCN() => sut.SetNonstrokingColorExtendedAsync(KnownNames.DeviceRGB, 1, 23);
    [Fact] public void BlockNonstrokinhG() => sut.SetNonstrokingGrayAsync(1);
    [Fact] public void BlockNonstrokinhRG() => sut.SetNonstrokingRgbAsync(1,2,3);
    [Fact] public void BlockNonstrokinhK() => sut.SetNonstrokingCMYKAsync(1,2,3, 4);
    [Fact] public void BlockRenderIntent() => sut.SetRenderIntent(RenderIntentName.Perceptual);
}