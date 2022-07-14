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
    
    protected (uint Color, bool IsBbackground) ColorFromT(double tParameter)
    {
        return (tParameter, extendLow, extendHigh) switch
        {
            (< 0, false, _) => (BackgroundColor, true),
            (< 0, true, _) =>  (ColorForValidT(0), false),
            (> 1, _, false) => (BackgroundColor, true),
            (> 1, _, true) =>  (ColorForValidT(1), false),
            var (t, _, _) =>   (ColorForValidT(t), false)
        };
    }

    private uint ColorForValidT(double t)
    {
        var mappedT = new ClosedInterval(0, 1).MapTo(domain, t);
        Span<double> rawColor = stackalloc double[ColorSpace.ExpectedComponents];
        function.Compute(mappedT, rawColor);
        var deviceColor = ColorSpace.SetColor(rawColor); 
        return deviceColor.AsArgbUint32();
    }

}