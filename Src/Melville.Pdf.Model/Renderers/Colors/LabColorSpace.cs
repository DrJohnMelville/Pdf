using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public class LabColorSpace : IColorSpace
{
    private readonly DeviceColor whitePoint;
    private readonly ClosedInterval aInterval;
    public readonly ClosedInterval bInterval;

    public LabColorSpace(DeviceColor whitePoint, ClosedInterval aInterval, ClosedInterval bInterval)
    {
        this.whitePoint = whitePoint;
        this.aInterval = aInterval;
        this.bInterval = bInterval;
    }

    public static async ValueTask<LabColorSpace> ParseAsync(PdfDictionary parameters)
    {
        var wp = await ReadWhitePoint(parameters);
        var array = await parameters.GetOrNullAsync(KnownNames.Range) is PdfArray arr
            ? await arr.AsDoublesAsync()
            : Array.Empty<double>();
        
        return new LabColorSpace(wp, 
            new ClosedInterval(TryGet(array, 0, - 100), TryGet(array, 1, 100)),
            new ClosedInterval(TryGet(array, 2, - 100), TryGet(array, 3, 100))
            );
    }

    private static async Task<DeviceColor> ReadWhitePoint(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint);
        return new DeviceColor(
            (await array.GetAsync<PdfNumber>(0)).DoubleValue,
            (await array.GetAsync<PdfNumber>(1)).DoubleValue,
            (await array.GetAsync<PdfNumber>(2)).DoubleValue
        );
    }

    private static double TryGet(double[]? arr, int index, double defaultValue) =>
        arr is not null && arr.Length > index ? arr[index] : defaultValue;

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 3)
            throw new PdfParseException("Wrong number of parameters for Lab color");
        var commonPart = (newColor[0] + 16) / 116;
        var L = commonPart + (aInterval.Clip(newColor[1]) / 500);
        var M = commonPart;
        var N = commonPart - (bInterval.Clip(newColor[2]) / 200);
        return XyzToDeviceColor.Transform(stackalloc float[]
        {
            (float)(whitePoint.Red * GFunc(L)),
            (float)(whitePoint.Green * GFunc(M)),
            (float)(whitePoint.Blue * GFunc(N))
        });
    }

    public double GFunc(double x) =>
        x >= (6.0 / 29) ? 
            x * x * x :
            (108.0 / 841) * (x - (4.0 / 29));
    
    public DeviceColor DefaultColor() => DeviceColor.Black;

    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor)
    {
        var sourceInterval = new ClosedInterval(0, 255);
        return SetColor(stackalloc double[]
        {
            newColor[0] * 100.0 / 255.0,
            sourceInterval.MapTo(aInterval, newColor[1]),
            sourceInterval.MapTo(bInterval, newColor[2]),
        });
    }
    
    public int ExpectedComponents => 3;
}