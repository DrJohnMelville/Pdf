using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class TestGraphicsState : GraphicsState<DeviceColor>
{
    protected override DeviceColor CreateSolidBrush(DeviceColor color) => color;
    protected override ValueTask<DeviceColor> CreatePatternBrush(PdfDictionary pattern,
        DocumentRenderer parentRenderer) => 
        throw new System.NotSupportedException();
}
public class GraphicStateDictionary
{

    [Fact]
    public void KeysHashCorrectly()
    {
        Assert.Equal(KnownNameKeys.LW, KnownNames.LW.GetHashCode());
        Assert.Equal(KnownNameKeys.LW, KnownNames.LW.GetHashCode());
    }
    [Fact]
    public async Task SetLineWidthWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LW, new PdfInteger(10));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(10.0, gs.Current().LineWidth);
    }
    [Fact]
    public async Task SetMiterLimitWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.ML, new PdfInteger(20));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(20.0, gs.Current().MiterLimit);
    }
    [Fact]
    public async Task SetFlatnessWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.FL, new PdfInteger(20));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(20.0, gs.Current().FlatnessTolerance);
    }
    [Fact]
    public async Task SetLineCapsWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LC, new PdfInteger((int)LineCap.Round));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(LineCap.Round, gs.Current().LineCap);
    }
    [Fact]
    public async Task SetLineJoinWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LJ, 
            new PdfInteger((int)LineJoinStyle.Round));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(LineJoinStyle.Round, gs.Current().LineJoinStyle);
    }
    [Fact]
    public async Task SetRenderIntentWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.RI, 
            RenderIntentName.Perceptual);
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(RenderIntentName.Perceptual, gs.Current().RenderIntent);
    }
    [Fact]
    public async Task SetDashStyleWithDictionary()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.D, 
            new PdfArray(
                new PdfArray(new PdfInteger(1), new PdfInteger(2)),
                new PdfInteger(3)
                ));
        var gs = await ComputeFinalGraphicsStack(page);
        Assert.Equal(3.0, gs.Current().DashPhase);
        Assert.Equal(1.0, gs.Current().DashArray[0]);
        Assert.Equal(2.0, gs.Current().DashArray[1]);
        Assert.Equal(2, gs.Current().DashArray.Length);
    }

    private static async Task<GraphicsStateStack<TestGraphicsState>> ComputeFinalGraphicsStack(PdfPage page)
    {
        var gs = new GraphicsStateStack<TestGraphicsState>();
        var target = new Mock<IRenderTarget>();
        target.SetupGet(i => i.GrapicsStateChange).Returns(gs);
        Assert.Equal(1.0, gs.Current().LineWidth);
        await new RenderEngine(page, target.Object, 
                DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance))
            .RunContentStream();
        return gs;
    }

    private static PdfPage PageThatSetsPropFromGSDictionary(GraphicStateParameterName property, PdfObject values)
    {
        var pageDict = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Page)
            .WithItem(KnownNames.Contents, new DictionaryBuilder().AsStream("/GSDict gs"))
            .WithItem(KnownNames.Resources, new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Resources)
                .WithItem(ResourceTypeName.ExtGState, new DictionaryBuilder()
                    .WithItem(NameDirectory.Get("GSDict"), new DictionaryBuilder()
                        .WithItem(KnownNames.Type, KnownNames.ExtGState)
                        .WithItem(property, values)
                        .AsDictionary())
                    .AsDictionary())
                .AsDictionary())
            .AsDictionary();
        var page = new PdfPage(pageDict);
        return page;
    }
}