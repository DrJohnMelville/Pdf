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

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal readonly struct Type1PdfFunctionShaderFactory
{
    private readonly PdfDictionary shadingDictionary;

    private static readonly RectInterval defaultDomainInterval = new (
        new ClosedInterval(0,1), new ClosedInterval(0,1));

    private static readonly RectInterval defaultBBox = new(
        new ClosedInterval(double.MinValue, double.MaxValue),
        new ClosedInterval(double.MinValue, double.MaxValue));

    public Type1PdfFunctionShaderFactory(PdfDictionary shadingDictionary)
    {
        this.shadingDictionary = shadingDictionary;
    }

    public async ValueTask<IShaderWriter> ParseAsync(CommonShaderValues common)
    {
        var domainToPattern = await (
                await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.MatrixTName).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        Matrix3x2.Invert(domainToPattern, out var patternToDomain);          


        var domainInterval = 
            (await ArrayParsingHelper.ReadFixedLengthDoubleArrayAsync(shadingDictionary, KnownNames.DomainTName, 4).CA()) is {} domainArray ?
                new RectInterval(new ClosedInterval(domainArray[0], domainArray[1]),
                new ClosedInterval(domainArray[2], domainArray[3])) : defaultDomainInterval;

        return new Type1PdfFunctionShader(
            common, patternToDomain, domainInterval,  
            await (await shadingDictionary[KnownNames.FunctionTName].CA()).CreateFunctionAsync().CA());
    }
}

internal class Type1PdfFunctionShader : PixelQueryFunctionalShader
{
    private readonly Matrix3x2 patternToDomain;
    private readonly RectInterval domainInterval;
    private readonly IPdfFunction function;

    public Type1PdfFunctionShader(
        in CommonShaderValues values, in Matrix3x2 patternToDomain, 
        RectInterval domainInterval, IPdfFunction function) 
        : base(values)
    {
        this.patternToDomain = patternToDomain;
        this.domainInterval = domainInterval;
        this.function = function;
        Debug.Assert(this.function.Domain.Length == 2);
        Debug.Assert(this.function.Range.Length == ColorSpace.ExpectedComponents);
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
        return ColorSpace.SetColor(nativeColor).AsPreMultiplied().AsArgbUint32();
    }
}