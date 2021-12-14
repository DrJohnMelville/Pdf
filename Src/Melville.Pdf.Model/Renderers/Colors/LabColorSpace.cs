using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.Colors;

public class LabColorSpace : IColorSpace
{
    private readonly DeviceColor whitePoint;

    public LabColorSpace(DeviceColor whitePoint)
    {
        this.whitePoint = whitePoint;
    }

    public static async ValueTask<LabColorSpace> Parse(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint);
        var wp = new DeviceColor(
            (await array.GetAsync<PdfNumber>(0)).DoubleValue,
            (await array.GetAsync<PdfNumber>(1)).DoubleValue,
            (await array.GetAsync<PdfNumber>(2)).DoubleValue
        );
        return new LabColorSpace(wp);
    }

    public DeviceColor SetColor(ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 3)
            throw new PdfParseException("Wrong number of parameters for CalGray color");
        var commonPart = (newColor[0] + 16) / 116;
        var L = commonPart + (newColor[1] / 500);
        var M = commonPart;
        var N = commonPart - (newColor[2] / 200);
        return XyzToDeviceColor.Transform(stackalloc float[]
        {
            (float)(whitePoint.Red * GFunc(L)),
            (float)(whitePoint.Green * GFunc(M)),
            (float)(whitePoint.Blue * GFunc(N))
        });
    }

    public double GFunc(double x) =>
        x >= (6.0 / 29) ? x * x * x : (108.0 / 841) * (x - (4.0 / 29));
}