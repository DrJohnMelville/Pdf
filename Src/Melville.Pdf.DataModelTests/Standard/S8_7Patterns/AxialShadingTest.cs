using System.Numerics;
using Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_7Patterns;

public class AxialShadingTest
{
    [Theory]
    [InlineData(0.0, 1, true, 0)]
    [InlineData(0.0, -1, true, 0)]
    [InlineData(-1, 0, true, 0)]
    [InlineData(1, 0, true, 0)]
    [InlineData(1, 1, true, 1)]
    [InlineData(1, -1, true, 1)]
    [InlineData(2, 0, true, 1)]
    [InlineData(0.35, 1, true, 0.35)]
    public void HorizontalRowTest(double x, double y, bool succeed, double t)
    {
        var comp = new AxialShadingComputer(0, 0, 1, 1, 0, 1);
        Assert.Equal(succeed, comp.TParameterFor(new Vector2((float)x,(float)y), out var computedT));
        Assert.Equal(t, computedT, 4);
    }

    [Theory]
    [InlineData(0.0, 1, true, 0)]
    [InlineData(0.0, -1, true, 0)]
    [InlineData(-1, 0, true, 0)]
    [InlineData(1, 0, true, 0)]
    [InlineData(1, 0.5, true, 1)]
    [InlineData(1, -0.5, true, 1)]
    [InlineData(1.5, 0, true, 1)]
    [InlineData(0.5, 0.75, true, 0.5)]
    public void SlopingLine(double x, double y, bool succeed, double t)
    {
        var comp = new AxialShadingComputer(0, 0, 1, 1, 0, 0.5);
        Assert.Equal(succeed, comp.TParameterFor(new Vector2((float)x,(float)y), out var computedT));
        Assert.Equal(t, computedT, 4);
    }

    [Theory]
    [InlineData(0.0, 1, true, 1)]
    [InlineData(0.0, -1, true, 1)]
    [InlineData(-1, 0, true, 1)]
    [InlineData(1, 0, true, 1)]
    [InlineData(1, 0.5, true, 0.6667)]
    [InlineData(1, -0.5, true, 0.6667)]
    [InlineData(1.5, 0, true, 0)]
    [InlineData(0.5, 0.75, true, 0.5)]
    public void SlopingLineInverse(double x, double y, bool succeed, double t)
    {
        var comp = new AxialShadingComputer( 1, 0, 0.5, 0, 0, 1);
        Assert.Equal(succeed, comp.TParameterFor(new Vector2((float)x,(float)y), out var computedT));
        Assert.Equal(t, computedT, 4);
    }
}