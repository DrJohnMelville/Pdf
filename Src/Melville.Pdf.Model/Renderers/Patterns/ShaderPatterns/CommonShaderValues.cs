using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.Model.Renderers.Colors;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal readonly record struct CommonShaderValues(
    Matrix3x2 PixelsToPattern, IColorSpace ColorSpace, RectInterval BBox, uint BackgroundColor)
{
    
    private static readonly RectInterval defaultBBox = new(
        new ClosedInterval(double.MinValue, double.MaxValue),
        new ClosedInterval(double.MinValue, double.MaxValue));

    public static async ValueTask<CommonShaderValues> ParseAsync(Matrix3x2 patternToPixels, PdfValueDictionary shadingDictionary,
        bool supressBackground)
    {
        Matrix3x2.Invert(patternToPixels, out var pixelsToPattern);
        var bbox =
            (await shadingDictionary.ReadFixedLengthDoubleArrayAsync(KnownNames.BBoxTName, 4).CA()) is { } bbArray
                ? new RectInterval(new ClosedInterval(bbArray[0], bbArray[2]),
                    new ClosedInterval(bbArray[1], bbArray[3]))
                : defaultBBox;


        var colorSpace = await new ColorSpaceFactory(NoPageContext.Instance)
            .FromNameOrArrayAsync(await shadingDictionary[KnownNames.ColorSpaceTName].CA()).CA();
        var backGroundInt = await ComputeBackgroundAsync(shadingDictionary, colorSpace, supressBackground).CA();

        var common = new CommonShaderValues(
            pixelsToPattern, colorSpace, bbox, backGroundInt);
        return common;
    }

    private static async Task<uint> ComputeBackgroundAsync(
        PdfValueDictionary shadingDictionary, IColorSpace colorSpace, bool supressBackground)
    {
        if (supressBackground) return 0;        
        var backGroundArray = 
            await shadingDictionary.GetOrNullAsync<PdfValueArray>(KnownNames.BackgroundTName).CA() is { } arr
            ? await arr.CastAsync<double>().CA()
            : Array.Empty<double>();
        var backGroundInt = ComputeBackgroundUint(backGroundArray, colorSpace);
        return backGroundInt;
    }

    private static uint ComputeBackgroundUint(double[] backGroundArray, IColorSpace colorSpace)
    {
        return (backGroundArray.Length == colorSpace.ExpectedComponents?
            colorSpace.SetColor(backGroundArray).AsPreMultiplied():DeviceColor.Invisible)
            .AsArgbUint32();
    }
}