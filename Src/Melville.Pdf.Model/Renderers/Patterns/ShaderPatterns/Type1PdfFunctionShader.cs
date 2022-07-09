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

    private static readonly RectInterval defaultDomainInterval = new (
        new ClosedInterval(0,1), new ClosedInterval(0,1));

    private static readonly RectInterval defaultBBox = new(
        new ClosedInterval(double.MinValue, double.MaxValue),
        new ClosedInterval(double.MinValue, double.MaxValue));

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
        Matrix3x2.Invert(domainToPattern, out var patternToDomain);          
         Matrix3x2.Invert(patternToPixels, out var pixelsToPattern);

        var colorSpace = await new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArray(await shadingDictionary[KnownNames.ColorSpace].CA()).CA();

        var domainInterval = 
            (await ReadFourDoubleArray(shadingDictionary, KnownNames.Domain).CA()) is {} domainArray ?
                new RectInterval(new ClosedInterval(domainArray[0], domainArray[1]),
                new ClosedInterval(domainArray[2], domainArray[3])) : defaultDomainInterval;

        var bbox =
            (await ReadFourDoubleArray(shadingDictionary, KnownNames.BBox).CA()) is { } bbArray
                ? new RectInterval(new ClosedInterval(bbArray[0], bbArray[2]),
                    new ClosedInterval(bbArray[1], bbArray[3]))
                : defaultBBox;

        
        var backGroundArray = await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.Background).CA() is {} arr ?
            await arr.AsDoublesAsync().CA() :
            Array.Empty<double>();


        return new Type1PdfFunctionShader(pixelsToPattern, patternToDomain,
            colorSpace,
            domainInterval, bbox,
            await (await shadingDictionary[KnownNames.Function].CA()).CreateFunctionAsync().CA(),
            ComputeBackgroundUint(backGroundArray, colorSpace)
        );
    }

    private static async ValueTask<double[]?> ReadFourDoubleArray(PdfDictionary dict, PdfName name) =>
        (await dict.GetOrNullAsync<PdfArray>(name).CA()) is
        { } pdfArray && await pdfArray.AsDoublesAsync().CA() is { Length: 4 } ret
            ? ret
            : null;

    private static uint ComputeBackgroundUint(double[] backGroundArray, IColorSpace colorSpace)
    {
        return (backGroundArray.Length == colorSpace.ExpectedComponents?colorSpace.SetColor(backGroundArray):DeviceColor.Invisible).AsArgbUint32();
    }
}

public class Type1PdfFunctionShader : IShaderWriter
{
    private readonly Matrix3x2 pixelsToPattern;
    private readonly Matrix3x2 patternToDomain;
    private readonly IColorSpace colorSpace;
    private readonly RectInterval domainInterval;
    private readonly RectInterval bboxInterval;
    private readonly IPdfFunction function;
    private readonly uint backgroundColor;

    public Type1PdfFunctionShader(
        in Matrix3x2 pixelsToPattern, in Matrix3x2 patternToDomain, IColorSpace colorSpace,
        RectInterval domainInterval,
        RectInterval bboxInterval,
        IPdfFunction function, uint backgroundColor)
    {
        this.pixelsToPattern = pixelsToPattern;
        this.patternToDomain = patternToDomain;
        this.colorSpace = colorSpace;
        this.domainInterval = domainInterval;
        this.bboxInterval = bboxInterval;
        this.function = function;
        this.backgroundColor = backgroundColor;
        
        Debug.Assert(this.function.Domain.Length == 2);
        Debug.Assert(this.function.Range.Length == colorSpace.ExpectedComponents);
    }

    public unsafe void RenderBits(uint* bits, int width, int height)
    {
        uint* pos = bits;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                uint desired = 0;
                desired = ColorForPixel(new Vector2(j, i));

                *pos++ = desired;
            }
        }
    }

    private uint ColorForPixel(in Vector2 pixel)
    {
        var patternVal = Vector2.Transform(pixel, pixelsToPattern);
        if (bboxInterval.OutOfRange(patternVal)) return 0;
        var domainVal = Vector2.Transform(patternVal, patternToDomain);
        return domainInterval.OutOfRange(domainVal)
            ? backgroundColor
            : ComputeColorFromFunction(domainVal);
    }

    private uint ComputeColorFromFunction(in Vector2 domainVal)
    {
        Span<double> source = stackalloc double[2];
        Span<double> nativeColor = stackalloc double[function.Range.Length];
        source[0] = domainVal.X;
        source[1] = domainVal.Y;
        function.Compute(source, nativeColor);
        return colorSpace.SetColor(nativeColor).AsArgbUint32();
    }
}

public readonly struct RectInterval
{
    public readonly ClosedInterval Horizontal { get; }
    public readonly ClosedInterval Vertical { get; }

    public RectInterval(ClosedInterval horizontal, ClosedInterval vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public bool OutOfRange(in Vector2 point) =>
        Horizontal.OutOfInterval(point.X) || Vertical.OutOfInterval(point.Y);
}