using System;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public abstract class ParametricFunctionalShader : PixelQueryFunctionalShader
{
    private readonly ClosedInterval domain;
    private readonly IPdfFunction function;
    private readonly bool extendLow;
    private readonly bool extendHigh;

    protected ParametricFunctionalShader(in CommonShaderValues values, ClosedInterval domain, IPdfFunction function, bool extendLow, bool extendHigh) : base(in values)
    {
        this.domain = domain;
        this.function = function;
        this.extendLow = extendLow;
        this.extendHigh = extendHigh;

        if (this.function.Range.Length != ColorSpace.ExpectedComponents)
            throw new PdfParseException("Function and colorspace mismatch in type 2 or 3 shader.");
    }

    protected override uint GetColorFromShader(Vector2 patternVal)
    {
        if (!TParameterFor(patternVal, out var tParameter)) return BackgroundColor;
        return (tParameter, extendLow, extendHigh) switch
        {
            (< 0, false, _) => BackgroundColor,
            (< 0, true, _) => ColorForT(0),
            (> 1, _, false) => BackgroundColor,
            (> 1, _, true) => ColorForT(1),
            var (t, _, _) => ColorForT(t)
        };
    }

    private uint ColorForT(double t)
    {
        var mappedT = new ClosedInterval(0, 1).MapTo(domain, t);
        Span<double> rawColor = stackalloc double[ColorSpace.ExpectedComponents];
        function.Compute(mappedT, rawColor);
        var deviceColor = ColorSpace.SetColor(rawColor);
        if (deviceColor.RedByte == 0)
        {
            ;
        }
        return deviceColor.AsArgbUint32();
    }

    protected abstract bool TParameterFor(Vector2 patternVal, out double T);
}