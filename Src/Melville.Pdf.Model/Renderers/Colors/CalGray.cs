using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public class CalGray : IColorSpace
{
    private readonly DeviceColor whitePoint;
    private readonly double gamma;

    public CalGray(DeviceColor whitePoint, double gamma)
    {
        this.whitePoint = whitePoint;
        this.gamma = gamma;
    }

    public static async ValueTask<CalGray> Parse(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint);
        var wp = new DeviceColor(
            (await array.GetAsync<PdfNumber>(0)).DoubleValue,
            (await array.GetAsync<PdfNumber>(1)).DoubleValue,
            (await array.GetAsync<PdfNumber>(2)).DoubleValue
        );
        var gamma = await parameters.GetOrDefaultAsync(KnownNames.Gamma, 1.0);
        return new CalGray(wp, gamma);
    }

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 1)
            throw new PdfParseException("Wrong number of parameters for CalGray color");
        var gammaTransformed = Math.Pow(newColor[0], gamma);
        return XyzToDeviceColor.Transform(stackalloc float[]
        {
            (float)(whitePoint.Red * gammaTransformed),
            (float)(whitePoint.Green * gammaTransformed),
            (float)(whitePoint.Blue * gammaTransformed),
        });

    }

    public DeviceColor DefaultColor() => DeviceColor.Black;
}