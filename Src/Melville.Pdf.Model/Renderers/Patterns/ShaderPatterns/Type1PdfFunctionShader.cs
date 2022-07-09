using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly struct Type1PdfFunctionShaderFactory
{
    private readonly PdfDictionary patternDictionary;
    private readonly PdfDictionary shadingDictionary;

    private static readonly double[] defaultDomainArray = { 0.0, 1.0, 0, 1.0 };

    public Type1PdfFunctionShaderFactory(PdfDictionary patternDictionary, PdfDictionary shadingDictionary)
    {
        this.patternDictionary = patternDictionary;
        this.shadingDictionary = shadingDictionary;
    }

    public async ValueTask<IShaderWriter> Parse()
    {
        var domainToPattern = await (await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.Matrix).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        var patternToPixels = await (await patternDictionary.GetOrNullAsync<PdfArray>(KnownNames.Matrix).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        var domainToPixels = domainToPattern * patternToPixels;
        Matrix3x2.Invert(domainToPixels, out var pixelsToDomain);

        var colorSpace = await new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArray(await shadingDictionary[KnownNames.ColorSpace].CA()).CA();

        var domainArray = (await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.Domain).CA()) is
                          { } pdfArray &&
                          (await pdfArray.AsDoublesAsync().CA()) is { Length: 4 } doubleArr
            ? doubleArr : defaultDomainArray;

        var backGroundArray = await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.Background).CA() is {} arr ?
            await arr.AsDoublesAsync().CA() :
            Array.Empty<double>();


        return new Type1PdfFunctionShader(pixelsToDomain,
            colorSpace,
            new ClosedInterval(domainArray[0], domainArray[1]),
            new ClosedInterval(domainArray[2], domainArray[3]),
            await FunctionFactory.CreateFunctionAsync(await shadingDictionary[KnownNames.Function].CA()).CA(),
            ComputeBackgroundUint(backGroundArray, colorSpace)
        );
    }

    private static uint ComputeBackgroundUint(double[] backGroundArray, IColorSpace colorSpace)
    {
        return (backGroundArray.Length == colorSpace.ExpectedComponents?colorSpace.SetColor(backGroundArray):DeviceColor.Invisible).AsArgbUint32();
    }
}

public class Type1PdfFunctionShader : IShaderWriter
{
    private readonly Matrix3x2 pixelsTodomain;
    private readonly IColorSpace colorSpace;
    private readonly ClosedInterval xDomain;
    private readonly ClosedInterval yDomain;
    private readonly IPdfFunction function;
    private readonly uint backgroundColor;

    public Type1PdfFunctionShader(Matrix3x2 pixelsTodomain, IColorSpace colorSpace, 
        ClosedInterval xDomain, ClosedInterval yDomain, 
        IPdfFunction function, uint backgroundColor)
    {
        this.pixelsTodomain = pixelsTodomain;
        this.colorSpace = colorSpace;
        this.xDomain = xDomain;
        this.yDomain = yDomain;
        this.function = function;
        this.backgroundColor = backgroundColor;
        
        Debug.Assert(this.function.Domain.Length == 2);
        Debug.Assert(this.function.Range.Length == colorSpace.ExpectedComponents);
    }

    public unsafe void RenderBits(uint* bits, int width, int height)
    {
        Span<double> source = stackalloc double[2];
        Span<double> nativeColor = stackalloc double[function.Range.Length];
        uint* pos = bits;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var domainVal = Vector2.Transform(new Vector2(j, i), pixelsTodomain);
                if (xDomain.OutOfInterval(domainVal.X) || yDomain.OutOfInterval(domainVal.Y))
                {
                    *pos++ = backgroundColor;
                }
                else
                {
                    source[0] = domainVal.X;
                    source[1] = domainVal.Y;
                    function.Compute(source, nativeColor);
                    *pos++ = colorSpace.SetColor(nativeColor).AsArgbUint32();
                }
            }
        }
    }
}

public readonly record struct IntInterval(int MinValue, int MaxValue);