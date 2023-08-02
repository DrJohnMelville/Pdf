using System;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Pdf.Model.Renderers.Colors.Profiles;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class CmykIntegrationTests
{
    [Theory]
    [InlineData(1, 1, 1, 1, 0, 0, 0)]
    [InlineData(0,0,0,0.5, 60.87, -0.1747, 0.396484)]
    [InlineData(0.2, 0.2, 0.2, 0, 79.254, 2.906250, 3.031250)]
    [InlineData(0.1, 0.2, 0.3, 0.4, 56.172641, 5.169547, 13.35622)]
    public async Task ICCToLabProfileAsync(float c, float m, float y, float k, float l, float a, float b)
    {
        var xform = (await CmykIccProfile.ReadCmykProfileAsync()).DeviceToPcsTransform(RenderIntent.Perceptual);
        var result = new float[3];
        xform!.Transform(stackalloc float[]{c,m,y,k}, result.AsSpan());
        Assert.Equal(l, result[0], 0, MidpointRounding.AwayFromZero);
        Assert.Equal(a, result[1], 2, MidpointRounding.AwayFromZero);
        Assert.Equal(b, result[2], 2, MidpointRounding.AwayFromZero);
        
    }
}