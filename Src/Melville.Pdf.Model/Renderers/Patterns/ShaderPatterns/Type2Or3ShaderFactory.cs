using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal readonly struct Type2Or3ShaderFactory
{
    private readonly PdfDictionary shading;

    public Type2Or3ShaderFactory(PdfDictionary shading)
    {
        this.shading = shading;
    }

    public async ValueTask<IShaderWriter> ParseAsync(CommonShaderValues common, int expectedCoords)
    {
        var coords = (await shading.ReadFixedLengthDoubleArrayAsync(KnownNames.Coords, expectedCoords).CA()) ??
                     throw new PdfParseException("Cannot find coords for axial or radia shader");

        var domain = (await shading.ReadFixedLengthDoubleArrayAsync(KnownNames.Domain, 2).CA()) is { } arr
            ? new ClosedInterval(arr[0], arr[1])
            : new ClosedInterval(0, 1);

        var function = await (await shading[KnownNames.Function].CA()).CreateFunctionAsync().CA();

        var (extendLow, extendHigh) =
            (await shading.GetOrNullAsync<PdfArray>(KnownNames.Extend).CA()) is { Count: 2 } extArr
                ? (await ElementIsTrueAsync(extArr, 0).CA(), await ElementIsTrueAsync(extArr, 1).CA())
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

    private async ValueTask<bool> ElementIsTrueAsync(PdfArray extArr, int index) =>
        (await extArr[index].CA()) == PdfBoolean.True;
}