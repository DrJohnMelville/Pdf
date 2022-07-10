using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly struct Type2AxialShaderFactory
{
    private readonly PdfDictionary shading;

    public Type2AxialShaderFactory(PdfDictionary shading)
    {
        this.shading = shading;
    }

    public async ValueTask<IShaderWriter> Parse(CommonShaderValues common)
    {
        var coords = (await shading.ReadFixedLengthDoubleArray(KnownNames.Coords, 4).CA()) ??
                     throw new PdfParseException("Cannot find coords for axial shader");

        var domain = (await shading.ReadFixedLengthDoubleArray(KnownNames.Domain, 2).CA()) is { } arr
            ? new ClosedInterval(arr[0], arr[1])
            : new ClosedInterval(0, 1);

        var function = await (await shading[KnownNames.Function].CA()).CreateFunctionAsync().CA();

        var (extendLow, extendHigh) =
            (await shading.GetOrNullAsync<PdfArray>(KnownNames.Extend).CA()) is { Count: 2} extArr
                ? (await ElementIsTrue(extArr, 0).CA(), await ElementIsTrue(extArr, 1).CA())
                : (false, false);

        return new Type2AxialShading(common, coords[0], coords[1], coords[2], coords[3],
            domain, function, extendLow, extendHigh);
    }

    private async ValueTask<bool> ElementIsTrue(PdfArray extArr, int index) =>
        (await extArr[index].CA()) == PdfBoolean.True;
}

public class Type2AxialShading: PixelQueryFunctionalShader
{
    private readonly double xBase, yBase;
    private readonly ClosedInterval domain;
    private readonly IPdfFunction function;
    private readonly bool extendLow;
    private readonly bool extendHigh;

    private readonly double xDelta, yDelta, denominator;

    public Type2AxialShading(CommonShaderValues common, 
        double xBase, double yBase, double xEnd, double yEnd, ClosedInterval domain, 
        IPdfFunction function, bool extendLow, bool extendHigh): base(common)
    {
        this.xBase = xBase;
        this.yBase = yBase;
        this.domain = domain;
        this.function = function;
        this.extendLow = extendLow;
        this.extendHigh = extendHigh;

        xDelta = xEnd - xBase;
        yDelta = yEnd - yBase;
        denominator = (xDelta * xDelta) + (yDelta * yDelta);

        if (this.function.Range.Length != ColorSpace.ExpectedComponents)
            throw new PdfParseException("Function and colorspace mismatch in type 2 shader.");
    }

    protected override uint GetColorFromShader(Vector2 patternVal) =>
        (ComputeTForPerpindicularPoint(patternVal), extendLow, extendHigh) switch
        {
            (< 0, false, _) => BackgroundColor,
            (< 0, true, _) => ColorForT(0),
            (> 1, _, false) => BackgroundColor,
            (> 1, _, true) => ColorForT(1),
            var (t, _, _) => ColorForT(t)
        };

    private double ComputeTForPerpindicularPoint(Vector2 patternVal) => 
        ((xDelta * (patternVal.X - xBase)) + (yDelta * (patternVal.Y - yBase))) / denominator;
    // this formula comes from section 8.7.4.5.3 in the PDF Spec

    private uint ColorForT(double t)
    {
        var mappedT = new ClosedInterval(0, 1).MapTo(domain, t);
        Span<double> rawColor = stackalloc double[ColorSpace.ExpectedComponents];
        function.Compute(mappedT, rawColor);
        return ColorSpace.SetColor(rawColor).AsArgbUint32();
    }
}