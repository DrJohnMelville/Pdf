using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.DocumentPartCaches;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Model.Renderers.OptionalContents;
using Moq;
using Xunit;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public class TestGraphicsState : GraphicsState<DeviceColor>
{
    protected override DeviceColor CreateSolidBrush(DeviceColor color) => color;
    protected override ValueTask<DeviceColor> CreatePatternBrushAsync(PdfValueDictionary pattern,
        DocumentRenderer parentRenderer) => 
        throw new System.NotSupportedException();
}
public class GraphicStateDictionary
{

    [Fact]
    public async Task SetLineWidthWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LW, 10);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(10.0, gs.StronglyTypedCurrentState().LineWidth);
    }
    [Fact]
    public async Task SetMiterLimitWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.ML, 20);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(20.0, gs.StronglyTypedCurrentState().MiterLimit);
    }
    [Fact]
    public async Task SetFlatnessWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.FL, 20);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(20.0, gs.StronglyTypedCurrentState().FlatnessTolerance);
    }
    [Fact]
    public async Task SetLineCapsWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LC, (int)LineCap.Round);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(LineCap.Round, gs.StronglyTypedCurrentState().LineCap);
    }
    [Fact]
    public async Task SetLineJoinWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.LJ, 
            (int)LineJoinStyle.Round);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(LineJoinStyle.Round, gs.StronglyTypedCurrentState().LineJoinStyle);
    }
    [Fact]
    public async Task SetRenderIntentWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.RI, 
            RenderIntentName.Perceptual);
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(RenderIntentName.Perceptual, gs.StronglyTypedCurrentState().RenderIntent);
    }
    [Fact]
    public async Task SetDashStyleWithDictionaryAsync()
    {
        var page = PageThatSetsPropFromGSDictionary(GraphicStateParameterName.D, 
            new PdfValueArray(
                new PdfValueArray(1, 2),
                3
                ));
        var gs = await ComputeFinalGraphicsStackAsync(page);
        Assert.Equal(3.0, gs.StronglyTypedCurrentState().DashPhase);
        Assert.Equal(1.0, gs.StronglyTypedCurrentState().DashArray[0]);
        Assert.Equal(2.0, gs.StronglyTypedCurrentState().DashArray[1]);
        Assert.Equal(2, gs.StronglyTypedCurrentState().DashArray.Length);
    }

    private static async Task<GraphicsStateStack<TestGraphicsState>> ComputeFinalGraphicsStackAsync(PdfPage page)
    {
        var gs = new GraphicsStateStack<TestGraphicsState>();
        var target = new Mock<IRenderTarget>();
        target.SetupGet(i => i.GraphicsState).Returns(()=>gs.StronglyTypedCurrentState());
        Assert.Equal(1.0, gs.StronglyTypedCurrentState().LineWidth);
        await new RenderEngine(page, new(target.Object, 
                DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance),
                NullOptionalContentCounter.Instance))
            .RunContentStreamAsync();
        return gs;
    }

    private static PdfPage PageThatSetsPropFromGSDictionary(GraphicStateParameterName property, PdfDirectValue values)
    {
        var pageDict = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.PageTName)
            .WithItem(KnownNames.ContentsTName, new ValueDictionaryBuilder().AsStream("/GSDict gs"))
            .WithItem(KnownNames.ResourcesTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.TypeTName, KnownNames.ResourcesTName)
                .WithItem(ResourceTypeName.ExtGState, new ValueDictionaryBuilder()
                    .WithItem(PdfDirectValue.CreateName("GSDict"), new ValueDictionaryBuilder()
                        .WithItem(KnownNames.TypeTName, KnownNames.ExtGStateTName)
                        .WithItem(property, values)
                        .AsDictionary())
                    .AsDictionary())
                .AsDictionary())
            .AsDictionary();
        var page = new PdfPage(pageDict);
        return page;
    }
}