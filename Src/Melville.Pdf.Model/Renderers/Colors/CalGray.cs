using System;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.Icc.Model.Tags;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

internal class CalGray : IColorSpace
{
    private readonly DoubleColor whitePoint;
    private readonly IColorTransform xyzToDeviceTransform;
    private readonly double gamma;

    public CalGray(DoubleColor whitePoint, double gamma)
    {
        this.whitePoint = whitePoint;
        xyzToDeviceTransform = XyzToRgbTransformFactory.Create(whitePoint);
        this.gamma = gamma;
    }

    public static async ValueTask<IColorSpace> ParseAsync(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint).CA();
        var wp = await array.AsDoubleColorAsync().CA();
        var gamma = await parameters.GetOrDefaultAsync(KnownNames.Gamma, 1.0).CA();
        return new CalGray(wp, gamma);
    }

    public DeviceColor SetColor(in ReadOnlySpan<double> newColor)
    {
        if (newColor.Length != 1)
            throw new PdfParseException("Wrong number of parameters for CalGray color");
        var gammaTransformed = Math.Pow(newColor[0], gamma);
        return xyzToDeviceTransform.ToDeviceColor(stackalloc float[]
        {
            (float)(whitePoint.Red * gammaTransformed),
            (float)(whitePoint.Green * gammaTransformed),
            (float)(whitePoint.Blue * gammaTransformed),
        });
    }

    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);

    public int ExpectedComponents => 1;

    private static ClosedInterval[] outputIntervals = { new(0, 1) };
    public ClosedInterval[] ColorComponentRanges(int bitsPerComponent) => outputIntervals;
}