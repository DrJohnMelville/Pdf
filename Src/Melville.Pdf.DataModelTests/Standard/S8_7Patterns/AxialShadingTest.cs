using System.Numerics;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class AxialShadingTest
{
    [Test]
    [Arguments(0.0, 1, 0, 0)]
    [Arguments(0.0, -1, 0, 0)]
    [Arguments(-1, 0, -2, 0)]
    [Arguments(1, 0, 0, 2)]
    [Arguments(1, 1, 1, 1)]
    [Arguments(1, -1, 1, 1)]
    [Arguments(2, 0, 1, 3)]
    [Arguments(0.35, 1, 0.35, 0.35)]
    public void HorizontalRowTest(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer(new double[]{0, 0, 1, 1, 0, 1});
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }

    [Test]
    [Arguments(0.0, 1, -1.3333, 0)]
    [Arguments(0.0, -1, -1.3333, 0)]
    [Arguments(-1, 0, -4, 0)]
    [Arguments(1, 0, 0, 1.3333)]
    [Arguments(1, 0.5, 0.3333, 1)]
    [Arguments(1, -0.5, 0.3333, 1)]
    [Arguments(1.5, 0, 1, 1.6667)]
    [Arguments(0.5, 0.75, -0.5 , 0.5)]
    public void SlopingLine(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer(new[]{0, 0, 1, 1, 0, 0.5});
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }

    [Test]
    [Arguments(0.0, 1, 1, 2.3333)]
    [Arguments(0.0, -1, 1, 2.3333)]
    [Arguments(-1, 0, 1, 5)]
    [Arguments(1, 0, -.3333, 1)]
    [Arguments(1, 0.5, 0, 0.6667)]
    [Arguments(1, -0.5, 0, 0.6667)]
    [Arguments(1.5, 0, -0.6667, 0)]
    [Arguments(0.5, 0.75, 0.5 , 1.5)]
    public void SlopingLineInverse(double x, double y, double low, double high)
    {
        var comp = new RadialShadingComputer(new[]{1, 0, 0.5, 0, 0, 1});
        Assert.True(comp.TParameterFor(new Vector2((float)x,(float)y), out var lowVal, out var highVal));
        Assert.Equal(low, lowVal, 4);
        Assert.Equal(high, highVal, 4);
    }
}