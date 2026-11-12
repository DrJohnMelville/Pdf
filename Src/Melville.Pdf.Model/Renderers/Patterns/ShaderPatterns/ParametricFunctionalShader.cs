using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal abstract class ParametricFunctionalShader : PixelQueryFunctionalShader
{
    private static readonly DeviceColor NotComputedYet = new (0xDE, 0xAD, 0xBE, 0xEF);
    private readonly ClosedInterval domain;
    private readonly IPdfFunction function;
    private readonly bool extendLow;
    private readonly bool extendHigh;
    private readonly DeviceColor[] buffer = new DeviceColor[1001];

    protected ParametricFunctionalShader(in CommonShaderValues values, ClosedInterval domain, IPdfFunction function, bool extendLow, bool extendHigh) : base(in values)
    {
        this.domain = domain;
        this.function = function;
        this.extendLow = extendLow;
        this.extendHigh = extendHigh;

        if (this.function.Range.Length != ColorSpace.ExpectedComponents)
            throw new PdfParseException("Function and colorspace mismatch in type 2 or 3 shader.");
        
        buffer.AsSpan().Fill(NotComputedYet);
    }
    
    protected (DeviceColor Color, bool IsBbackground) ColorFromT(double tParameter)
    {
        return (tParameter, extendLow, extendHigh) switch
        {
            (< 0, false, _) => (BackgroundColor, true),
            (< 0, true, _) =>  (ColorForValidT(0), true),
            (> 1, _, false) => (BackgroundColor, true),
            (> 1, _, true) =>  (ColorForValidT(1), true),
            var (t, _, _) =>   (ColorForValidT(t), false)
        };
    }
    
    

    private DeviceColor ColorForValidT(double t)
    {
        var mappedT = new ClosedInterval(0, 1).MapTo(domain, t);
        return GetValue(ref BufferSlotForT(mappedT), mappedT);
    }

    private ref DeviceColor BufferSlotForT(double mappedT) => ref buffer[(int)(mappedT * 1000)];

    private DeviceColor GetValue(ref DeviceColor bufferSpot, double mappedT)
    {
        if (bufferSpot.Equals(NotComputedYet)) bufferSpot = ColorFroMappedT(mappedT);
        return bufferSpot;
    }

    private DeviceColor ColorFroMappedT(double mappedT)
    {
        Span<double> rawColor = stackalloc double[ColorSpace.ExpectedComponents];
        function.Compute(mappedT, rawColor);
        var deviceColor = ColorSpace.SetColor(rawColor);
        return deviceColor.AsPreMultiplied();
    }
}