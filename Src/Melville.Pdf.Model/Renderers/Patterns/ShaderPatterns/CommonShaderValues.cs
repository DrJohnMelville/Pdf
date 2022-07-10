using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public readonly record struct CommonShaderValues(
    Matrix3x2 PixelsToPattern, IColorSpace ColorSpace, RectInterval BBox, uint BackgroundColor)
{
    
    private static readonly RectInterval defaultBBox = new(
        new ClosedInterval(double.MinValue, double.MaxValue),
        new ClosedInterval(double.MinValue, double.MaxValue));

    public static async ValueTask<CommonShaderValues> Parse(PdfDictionary patternDictionary,
        PdfDictionary shadingDictionary)
    {
        var patternToPixels = await (await patternDictionary.GetOrNullAsync<PdfArray>(KnownNames.Matrix).CA())
            .AsMatrix3x2OrIdentityAsync().CA();
        Matrix3x2.Invert(patternToPixels, out var pixelsToPattern);
        var bbox =
            (await ArrayParsingHelper.ReadFixedLengthDoubleArray(shadingDictionary, KnownNames.BBox, 4).CA()) is { } bbArray
                ? new RectInterval(new ClosedInterval(bbArray[0], bbArray[2]),
                    new ClosedInterval(bbArray[1], bbArray[3]))
                : defaultBBox;


        var colorSpace = await new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArray(await shadingDictionary[KnownNames.ColorSpace].CA()).CA();
        var backGroundArray = await shadingDictionary.GetOrNullAsync<PdfArray>(KnownNames.Background).CA() is { } arr
            ? await arr.AsDoublesAsync().CA()
            : Array.Empty<double>();

        var common = new CommonShaderValues(
            pixelsToPattern, colorSpace, bbox, ComputeBackgroundUint(backGroundArray, colorSpace));
        return common;
    }
    
    private static uint ComputeBackgroundUint(double[] backGroundArray, IColorSpace colorSpace)
    {
        return (backGroundArray.Length == colorSpace.ExpectedComponents?colorSpace.SetColor(backGroundArray):DeviceColor.Invisible).AsArgbUint32();
    }
}