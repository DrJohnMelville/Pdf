using System;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.Colors.Profiles;
using SharpFont;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class CmykIntegrationTests
{
    [Theory]
    [InlineData(1, 1, 1, 1, 0, 0, 0)]
    [InlineData(0,0,0,0.5, 60.87, -0.1757, 0.36984)]
    public async Task ICCToLabProfile(float c, float m, float y, float k, float l, float a, float b)
    {
        var xform = (await IccProfileLibrary.ReadCmyk()).DeviceToPcsTransform(RenderIntent.Perceptual);
        var result = new float[3];
        xform!.Transform(stackalloc float[]{c,m,y,k}, result.AsSpan());
        Assert.Equal(l, result[0], 0);
        Assert.Equal(a, result[1], 2);
        Assert.Equal(b, result[2], 2);
        
    }

    [Theory]
    [InlineData(1, 1, 1, 1, 0, 0, 0)]
//    [InlineData(0, 0, 0, 0.5, 60.87, -0.1757, 0.36984)]
    public async Task CompositeTransform(float c, float m, float y, float k, float r, float g, float b)
    {
        var xform = (await IccProfileLibrary.ReadCmyk()).TransformTo(
            await IccProfileLibrary.ReadSrgb());
        var result = new float[3];
        xform!.Transform(stackalloc float[]{c,m,y,k}, result.AsSpan());
        Assert.Equal(r, result[0], 0);
        Assert.Equal(g, result[1], 2);
        Assert.Equal(b, result[2], 2);
    }
}