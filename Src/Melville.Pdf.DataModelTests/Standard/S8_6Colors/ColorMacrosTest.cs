using System.Drawing;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
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

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class ColorMacrosTest
{
    private readonly GraphicsStateStack<TestGraphicsState> state = new();
    private readonly Mock<IRenderTarget> target = new();
    private readonly RenderEngine sut;

    public ColorMacrosTest()
    {
        target.SetupGet(i => i.GraphicsState).Returns(()=>state.StronglyTypedCurrentState());
        var page = new PdfPage(new ValueDictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, new(target.Object, DocumentRendererFactory.CreateRenderer(page, WindowsDefaultFonts.Instance), 
            NullOptionalContentCounter.Instance));
    }

    [Fact]
    public async Task SetToDeviceGrayAsync()
    {
        await sut.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        sut.SetStrokeColor(0.75, 1, 1);
        await sut.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceGray);
        Assert.Equal(DeviceColor.FromDoubles(0,0,0), state.StronglyTypedCurrentState().StrokeColor);
        Assert.Equal(DeviceGray.Instance, state.StronglyTypedCurrentState().StrokeColorSpace);
        
        sut.SetStrokeColor(0.5);
        Assert.Equal(DeviceColor.FromDoubles(0.5,0.5,0.5), state.StronglyTypedCurrentState().StrokeColor);
    }

    [Fact]
    public async Task SetToDeviceRgbAsync()
    {
        sut.SetStrokeColor(0.75);
        await sut.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        Assert.Equal(DeviceColor.FromDoubles(0,0,0), state.StronglyTypedCurrentState().StrokeColor);
        Assert.Equal(DeviceRgb.Instance, state.StronglyTypedCurrentState().StrokeColorSpace);
        
        sut.SetStrokeColor(0.5, 0.6, 0.7);
        Assert.Equal(DeviceColor.FromDoubles(0.5,0.6,0.7), state.StronglyTypedCurrentState().StrokeColor);
    }
    [Fact]
    public async Task SetNonStrokeColorAsync()
    {
        sut.SetNonstrokingColor(0.75);
        await sut.SetNonstrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        Assert.Equal(DeviceColor.FromDoubles(0,0,0), state.StronglyTypedCurrentState().NonstrokeColor);
        Assert.Equal(DeviceRgb.Instance, state.StronglyTypedCurrentState().NonstrokeColorSpace);
        
        sut.SetNonstrokingColor(0.5, 0.6, 0.7);
        Assert.Equal(DeviceColor.FromDoubles(0.5,0.6,0.7), state.StronglyTypedCurrentState().NonstrokeColor);
    }
    [Fact]
    public async Task SetStrokeColorExtendedAsync()
    {
        await sut.SetStrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        await sut.SetStrokeColorExtendedAsync(null, 0.5, 0.6, 0.7);
        Assert.Equal(DeviceColor.FromDoubles(0.5,0.6,0.7), state.StronglyTypedCurrentState().StrokeColor);
    }
    [Fact]
    public async Task SetNonStrokeColorExtendedAsync()
    {
        await sut.SetNonstrokingColorSpaceAsync(ColorSpaceName.DeviceRGB);
        await sut.SetNonstrokingColorExtendedAsync(null, 0.5, 0.6, 0.7);
        Assert.Equal(DeviceColor.FromDoubles(0.5,0.6,0.7), state.StronglyTypedCurrentState().NonstrokeColor);
    }

    [Fact] public void GrayStrokeMacro()
    {
        sut.SetStrokeRGBAsync(1,1,1);
        sut.SetStrokeGrayAsync(0.4);
        Assert.Equal(DeviceGray.Instance, state.StronglyTypedCurrentState().StrokeColorSpace);
        Assert.Equal(DeviceColor.FromDoubles(0.4,0.4,0.4), state.StronglyTypedCurrentState().StrokeColor);
    }
    [Fact] public void RgbStrokeMacro()
    {
        sut.SetStrokeRGBAsync(0.4,0.5,0.6);
        Assert.Equal(DeviceRgb.Instance, state.StronglyTypedCurrentState().StrokeColorSpace);
        Assert.Equal(DeviceColor.FromDoubles(0.4,0.5,0.6), state.StronglyTypedCurrentState().StrokeColor);
    }
    [Fact] public async Task CmykStrokeMacroAsync()
    {
        await sut.SetStrokeCMYKAsync(0,0,0,0);
        Assert.Equal(await ColorSpaceFactory.CreateCmykColorSpaceAsync(), state.StronglyTypedCurrentState().StrokeColorSpace);
        VerifyWhite(state.StronglyTypedCurrentState().StrokeColor);
    }
    [Fact] public void GrayNonstrokingMacro()
    {
        sut.SetNonstrokingRgbAsync(1,1,1);
        sut.SetNonstrokingGrayAsync(0.4);
        Assert.Equal(DeviceGray.Instance, state.StronglyTypedCurrentState().NonstrokeColorSpace);
        Assert.Equal(DeviceColor.FromDoubles(0.4,0.4,0.4), state.StronglyTypedCurrentState().NonstrokeColor);
    }
    [Fact] public void RgbNonstrokingMacro()
    {
        sut.SetNonstrokingRgbAsync(0.4,0.5,0.6);
        Assert.Equal(DeviceRgb.Instance, state.StronglyTypedCurrentState().NonstrokeColorSpace);
        Assert.Equal(DeviceColor.FromDoubles(0.4,0.5,0.6), state.StronglyTypedCurrentState().NonstrokeColor);
    }
    [Fact] public async Task CmykNonstrokingMacroAsync()
    {
        await sut.SetNonstrokingCMYKAsync(0,0,0,0);
        Assert.Equal(await ColorSpaceFactory.CreateCmykColorSpaceAsync(), state.StronglyTypedCurrentState().NonstrokeColorSpace);
        VerifyWhite(state.StronglyTypedCurrentState().NonstrokeColor);
    }

    [Fact] public async Task Cmyk1111IsBlackAsync()
    {
        await sut.SetNonstrokingCMYKAsync(1.0,1.0,1.0, 1.0);
        Assert.Equal(await ColorSpaceFactory.CreateCmykColorSpaceAsync(), state.StronglyTypedCurrentState().NonstrokeColorSpace);
        Assert.Equal(new DeviceColor(0,0,0,255), state.StronglyTypedCurrentState().NonstrokeColor);
    }

    private static void VerifyWhite(DeviceColor color)
    {
        Assert.Equal(new DeviceColor(253, 253, 253, 255), color);
    }
}