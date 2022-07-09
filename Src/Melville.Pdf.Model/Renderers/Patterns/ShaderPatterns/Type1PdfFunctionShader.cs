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

public abstract class PixelQueryFunctionalShader: IShaderWriter
{
    private readonly Matrix3x2 pixelsToPattern;
    protected IColorSpace ColorSpace { get; }
    private readonly RectInterval bboxInterval;
    protected uint BackgroundColor { get; }

    protected PixelQueryFunctionalShader(
        in Matrix3x2 pixelsToPattern, IColorSpace colorSpace,
        RectInterval bboxInterval, uint backgroundColor)
    {
        this.pixelsToPattern = pixelsToPattern;
        ColorSpace = colorSpace;
        this.bboxInterval = bboxInterval;
        BackgroundColor = backgroundColor;
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
        return bboxInterval.OutOfRange(patternVal) ? 
            0 : 
            GetColorFromShader(patternVal);
    }

    protected abstract uint GetColorFromShader(Vector2 patternVal);
}
public class Type1PdfFunctionShader : PixelQueryFunctionalShader
{
    private readonly Matrix3x2 patternToDomain;
    private readonly RectInterval domainInterval;
    private readonly IPdfFunction function;

    public Type1PdfFunctionShader(
        in Matrix3x2 pixelsToPattern, in Matrix3x2 patternToDomain, IColorSpace colorSpace, 
        RectInterval domainInterval, RectInterval bboxInterval, IPdfFunction function, uint backgroundColor) 
        : base(in pixelsToPattern, colorSpace, bboxInterval, backgroundColor)
    {
        this.patternToDomain = patternToDomain;
        this.domainInterval = domainInterval;
        this.function = function;
        Debug.Assert(this.function.Domain.Length == 2);
        Debug.Assert(this.function.Range.Length == colorSpace.ExpectedComponents);
    }
    protected override uint GetColorFromShader(Vector2 patternVal)
    {
        var domainVal = Vector2.Transform(patternVal, patternToDomain);
        return domainInterval.OutOfRange(domainVal)
            ? BackgroundColor
            : ComputeColorFromFunction(domainVal);
    }

    private uint ComputeColorFromFunction(in Vector2 domainVal)
    {
        Span<double> source = stackalloc double[2];
        Span<double> nativeColor = stackalloc double[function.Range.Length];
        source[0] = domainVal.X;
        source[1] = domainVal.Y;
        function.Compute(source, nativeColor);
        return ColorSpace.SetColor(nativeColor).AsArgbUint32();
    }
}