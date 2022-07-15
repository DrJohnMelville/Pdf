using System.Numerics;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class AxialShadingTest
{
    [Theory]
    [InlineData(0.0, 1, 0, 0)]
    [InlineData(0.0, -1, 0, 0)]
    [InlineData(-1, 0, -2, 0)]
    [InlineData(1, 0, 0, 2)]
    [InlineData(1, 1, 1, 1)]
    [InlineData(1, -1, 1, 1)]
    [InlineData(2, 0, 1, 3)]
    [InlineData(0.35, 1, 0.35, 0.35)]
    public void HorizontalRowTest(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer(0, 0, 1, 1, 0, 1);
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }

    [Theory]
    [InlineData(0.0, 1, -1.3333, 0)]
    [InlineData(0.0, -1, -1.3333, 0)]
    [InlineData(-1, 0, -4, 0)]
    [InlineData(1, 0, 0, 1.3333)]
    [InlineData(1, 0.5, 0.3333, 1)]
    [InlineData(1, -0.5, 0.3333, 1)]
    [InlineData(1.5, 0, 1, 1.6667)]
    [InlineData(0.5, 0.75, -0.5 , 0.5)]
    public void SlopingLine(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer(0, 0, 1, 1, 0, 0.5);
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }

    [Theory]
    [InlineData(0.0, 1, 1, 2.3333)]
    [InlineData(0.0, -1, 1, 2.3333)]
    [InlineData(-1, 0, 1, 5)]
    [InlineData(1, 0, -.3333, 1)]
    [InlineData(1, 0.5, 0, 0.6667)]
    [InlineData(1, -0.5, 0, 0.6667)]
    [InlineData(1.5, 0, -0.6667, 0)]
    [InlineData(0.5, 0.75, 0.5 , 1.5)]
    public void SlopingLineInverse(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer( 1, 0, 0.5, 0, 0, 1);
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }
}