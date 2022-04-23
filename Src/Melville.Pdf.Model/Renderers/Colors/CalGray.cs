using System;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Colors;

public class CalGray : IColorSpace
{
    private readonly DoubleColor whitePoint;
    private readonly IColorTransform xyzToDeviceTransform;
    private readonly double gamma;

    public CalGray(DoubleColor whitePoint, double gamma)
    {
        this.whitePoint = whitePoint;
        xyzToDeviceTransform = new XyzToDeviceColor(whitePoint);
        this.gamma = gamma;
    }

    public static async ValueTask<IColorSpace> Parse(PdfDictionary parameters)
    {
        var array = await parameters.GetAsync<PdfArray>(KnownNames.WhitePoint).CA();
        var wp = await DoubleColor.ParseAsync(array).CA();
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
#warning need to adapt to d50  before doing the xyztransform.

    }

    public DeviceColor DefaultColor() => DeviceColor.Black;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) =>
        this.SetColorSingleFactor(newColor, 1.0 / 255.0);

    public int ExpectedComponents => 1;

    private ClosedInterval[] outputIntervals = { new(0, 1) };
    public ClosedInterval[] DefaultOutputIntervals(int bitsPerComponent) => outputIntervals;
}