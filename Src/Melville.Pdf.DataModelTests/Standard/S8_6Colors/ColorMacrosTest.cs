using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_6Colors;

public class ColorMacrosTest
{
    private readonly GraphicsStateStack state = new();
    private readonly Mock<IRenderTarget> target = new Mock<IRenderTarget>();
    private readonly RenderEngine sut;

    public ColorMacrosTest()
    {
        target.SetupGet(i => i.GrapicsStateChange).Returns(state);
        var page = new PdfPage(new DictionaryBuilder().AsDictionary());
        sut = new RenderEngine(page, target.Object);
    }

    [Fact]
    public async Task SetToDeviceGray()
    {
        await sut.SetStrokingColorSpace(ColorSpaceName.DeviceRGB);
        sut.SetStrokeColor(0.75, 1, 1);
        await sut.SetStrokingColorSpace(ColorSpaceName.DeviceGray);
        Assert.Equal(new DeviceColor(0,0,0), state.Current().StrokeColor);
        Assert.Equal(DeviceGray.Instance, state.Current().StrokeColorSpace);
        
        sut.SetStrokeColor(0.5);
        Assert.Equal(new DeviceColor(0.5,0.5,0.5), state.Current().StrokeColor);
    }

    [Fact]
    public async Task SetToDeviceRgb()
    {
        sut.SetStrokeColor(0.75);
        await sut.SetStrokingColorSpace(ColorSpaceName.DeviceRGB);
        Assert.Equal(new DeviceColor(0,0,0), state.Current().StrokeColor);
        Assert.Equal(DeviceRgb.Instance, state.Current().StrokeColorSpace);
        
        sut.SetStrokeColor(0.5, 0.6, 0.7);
        Assert.Equal(new DeviceColor(0.5,0.6,0.7), state.Current().StrokeColor);
    }
    [Fact]
    public async Task SetNonStrokeColor()
    {
        sut.SetNonstrokingColor(0.75);
        await sut.SetNonstrokingColorSpace(ColorSpaceName.DeviceRGB);
        Assert.Equal(new DeviceColor(0,0,0), state.Current().NonstrokeColor);
        Assert.Equal(DeviceRgb.Instance, state.Current().NonstrokeColorSpace);
        
        sut.SetNonstrokingColor(0.5, 0.6, 0.7);
        Assert.Equal(new DeviceColor(0.5,0.6,0.7), state.Current().NonstrokeColor);
    }
    [Fact]
    public async Task SetStrokeColorExtended()
    {
        await sut.SetStrokingColorSpace(ColorSpaceName.DeviceRGB);
        await sut.SetStrokeColorExtended(null, 0.5, 0.6, 0.7);
        Assert.Equal(new DeviceColor(0.5,0.6,0.7), state.Current().StrokeColor);
    }
    [Fact]
    public async Task SetNonStrokeColorExtended()
    {
        await sut.SetNonstrokingColorSpace(ColorSpaceName.DeviceRGB);
        await sut.SetNonstrokingColorExtended(null, 0.5, 0.6, 0.7);
        Assert.Equal(new DeviceColor(0.5,0.6,0.7), state.Current().NonstrokeColor);
    }

    [Fact] public void GrayStrokeMacro()
    {
        sut.SetStrokeRGB(1,1,1);
        sut.SetStrokeGray(0.4);
        Assert.Equal(DeviceGray.Instance, state.Current().StrokeColorSpace);
        Assert.Equal(new DeviceColor(0.4,0.4,0.4), state.Current().StrokeColor);
    }
    [Fact] public void RgbStrokeMacro()
    {
        sut.SetStrokeRGB(0.4,0.5,0.6);
        Assert.Equal(DeviceRgb.Instance, state.Current().StrokeColorSpace);
        Assert.Equal(new DeviceColor(0.4,0.5,0.6), state.Current().StrokeColor);
    }
    [Fact] public void CmykStrokeMacro()
    {
        sut.SetStrokeCMYK(0,0,0,0);
        Assert.Equal(DeviceCmyk.Instance, state.Current().StrokeColorSpace);
        Assert.Equal(new DeviceColor(1,1,1), state.Current().StrokeColor);
    }
    [Fact] public void GrayNonstrokingMacro()
    {
        sut.SetNonstrokingRGB(1,1,1);
        sut.SetNonstrokingGray(0.4);
        Assert.Equal(DeviceGray.Instance, state.Current().NonstrokeColorSpace);
        Assert.Equal(new DeviceColor(0.4,0.4,0.4), state.Current().NonstrokeColor);
    }
    [Fact] public void RgbNonstrokingMacro()
    {
        sut.SetNonstrokingRGB(0.4,0.5,0.6);
        Assert.Equal(DeviceRgb.Instance, state.Current().NonstrokeColorSpace);
        Assert.Equal(new DeviceColor(0.4,0.5,0.6), state.Current().NonstrokeColor);
    }
    [Fact] public void CmykNonstrokingMacro()
    {
        sut.SetNonstrokingCMYK(0,0,0,0);
        Assert.Equal(DeviceCmyk.Instance, state.Current().NonstrokeColorSpace);
        Assert.Equal(new DeviceColor(1,1,1), state.Current().NonstrokeColor);
    }
}