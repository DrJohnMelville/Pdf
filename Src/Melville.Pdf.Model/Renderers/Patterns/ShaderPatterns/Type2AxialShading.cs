using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly struct Type2Or3ShaderFactory
{
    private readonly PdfDictionary shading;

    public Type2Or3ShaderFactory(PdfDictionary shading)
    {
        this.shading = shading;
    }

    public async ValueTask<IShaderWriter> Parse(CommonShaderValues common, int expectedCoords)
    {
        var coords = (await shading.ReadFixedLengthDoubleArray(KnownNames.Coords, expectedCoords).CA()) ??
                     throw new PdfParseException("Cannot find coords for axial or radia shader");

        var domain = (await shading.ReadFixedLengthDoubleArray(KnownNames.Domain, 2).CA()) is { } arr
            ? new ClosedInterval(arr[0], arr[1])
            : new ClosedInterval(0, 1);

        var function = await (await shading[KnownNames.Function].CA()).CreateFunctionAsync().CA();

        var (extendLow, extendHigh) =
            (await shading.GetOrNullAsync<PdfArray>(KnownNames.Extend).CA()) is { Count: 2 } extArr
                ? (await ElementIsTrue(extArr, 0).CA(), await ElementIsTrue(extArr, 1).CA())
                : (false, false);

        return expectedCoords switch
        {
            4 => new Type2AxialShading(common, coords, domain, function, extendLow, extendHigh),
            6 => new Type3RadialShading(
                OptimizeTypeAlialBBox(common, coords, extendLow, extendHigh)
                , coords, domain, function, extendLow, extendHigh),
            _ => throw new PdfParseException("Incorrect number of coordinates for shaker.")
        };
    }

    private CommonShaderValues OptimizeTypeAlialBBox(
        CommonShaderValues common, double[] coords, bool extendLow, bool extendHigh)
    {
        if (coords[2] < Double.Epsilon*2) return AdjustBox(common, coords[3], coords[4], coords[5], extendHigh);
        if (coords[5] < double.Epsilon*2) return AdjustBox(common, coords[1], coords[1], coords[2], extendLow);
        return common;
    }

    private CommonShaderValues AdjustBox(
        CommonShaderValues common, double centerX, double centerY, double radius, bool extend)
    {
        if (extend) return common;
        return common with
        {
            BBox = common.BBox.Intersect(
                new RectInterval(new ClosedInterval(centerX - radius, centerX + radius),
                    new ClosedInterval(centerY - radius, centerY + radius))
            )
        };
    }

    private async ValueTask<bool> ElementIsTrue(PdfArray extArr, int index) =>
        (await extArr[index].CA()) == PdfBoolean.True;
}

public class Type2AxialShading : ParametricFunctionalShader
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