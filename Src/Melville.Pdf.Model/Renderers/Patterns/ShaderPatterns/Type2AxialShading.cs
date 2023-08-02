using System.Diagnostics;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal class Type2AxialShading : ParametricFunctionalShader
{
    private readonly double xBase, yBase;
    private readonly double xDelta, yDelta, denominator;

    public Type2AxialShading(CommonShaderValues common,
        double[] coords, ClosedInterval domain,
        IPdfFunction function, bool extendLow, bool extendHigh) : base(common,
        domain, function, extendLow, extendHigh)
    {
        Debug.Assert(coords.Length == 4);
        this.xBase = coords[0];
        this.yBase = coords[1];

        xDelta = coords[2] - xBase;
        yDelta = coords[3] - yBase;
        denominator = (xDelta * xDelta) + (yDelta * yDelta);
    }

    protected override uint GetColorFromShader(Vector2 patternVal)
    {
        var tParameter =((xDelta * (patternVal.X - xBase)) + (yDelta * (patternVal.Y - yBase))) / denominator;
        return ColorFromT(tParameter).Color;
    }


}