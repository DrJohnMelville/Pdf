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
    [InlineData(0,0,0,0.5, 60.87, -0.1747, 0.396484)]
    [InlineData(0.2, 0.2, 0.2, 0, 79.254, 2.906250, 3.031250)]
    [InlineData(0.1, 0.2, 0.3, 0.4, 56.172641, 5.169547, 13.35622)]
    public async Task ICCToLabProfile(float c, float m, float y, float k, float l, float a, float b)
    {
        var xform = (await IccProfileLibrary.ReadCmyk()).DeviceToPcsTransform(RenderIntent.Perceptual);
        var result = new float[3];
        xform!.Transform(stackalloc float[]{c,m,y,k}, result.AsSpan());
        Assert.Equal(l, result[0], 0);
        Assert.Equal(a, result[1], 2);
        Assert.Equal(b, result[2], 2);
        
    }
}