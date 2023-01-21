using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.ColorSpaces;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.Colors.Profiles;
using Xunit;

namespace Melville.Pdf.WpfToolTests.LowLevelViewer.ColorSpaceViewers;

public class ColorspaceViewModelFactoryTest
{
    [Fact]
    public async Task CanCreateInvalidProfile()
    {
        await ColorSpaceViewModelFactory.CreateAsync(new MemoryStream("Invalid Icc".AsExtendedAsciiBytes()));
        // assertion is that it does not throw
    }

    [Fact]
    public async Task CreateValidProfile()
    {
        var multiModel = await ColorSpaceViewModelFactory.CreateAsync(
            CmykIccProfile.GetCmykProfileStream());
        Assert.Equal(2, multiModel.Spaces.Count);
        VerifyCmyk(multiModel.Spaces[0]);

    }

    private void VerifyCmyk(ColorSpaceViewModel csvm)
    {
        Assert.NotNull(csvm);
        Assert.Equal("ICC Profile", csvm.Title);
        
        Assert.Equal(4, csvm.Axes.Count());
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(new ClosedInterval(0,1), csvm.Axes[i].Interval);
        }
        Assert.Equal(new DeviceColor(253,253,253,255), csvm.Color);
    }

    [Fact]
    public async Task MaxMinColor()
    {
        var model = await ColorSpaceViewModelFactory.CreateAsync(new MemoryStream());
        CheckMaxMin(model.Spaces[0]);
    }

    private void CheckMaxMin(ColorSpaceViewModel model)
    {
        model.Axes[0].Value = 0.5;
        Assert.Equal(new DeviceColor(127,0,0,255), model.Color);
        Assert.Equal(new DeviceColor(0, 0, 0, 255), model.Axes[0].MinColor);
        Assert.Equal(new DeviceColor(255, 0, 0, 255), model.Axes[0].MaxColor);
        Assert.Equal(new DeviceColor(127, 0, 0, 255), model.Axes[1].MinColor);
        Assert.Equal(new DeviceColor(127, 255, 0, 255), model.Axes[1].MaxColor);
        Assert.Equal(new DeviceColor(127, 0, 0, 255), model.Axes[2].MinColor);
        Assert.Equal(new DeviceColor(127, 0, 255, 255), model.Axes[2].MaxColor);
    }
}