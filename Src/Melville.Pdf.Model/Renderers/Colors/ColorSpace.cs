using System;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Renderers.Colors;

public record struct DeviceColor(double Red, double Green, double Blue)
{
    public static readonly DeviceColor Black = new(0, 0, 0);

    public byte RedByte => (byte)(255 * Red);
    public byte GreenByte => (byte)(255 * Green);
    public byte BlueByte => (byte)(255 * Blue);
}

public interface IColorSpace
{
    public DeviceColor SetColor(ReadOnlySpan<double> newColor);
}

public static class ColorSpaceFactory
{
    public static ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, in PdfPage page)
    {
        return colorSpaceName.GetHashCode() switch
        {
            KnownNameKeys.DeviceGray => new(DeviceGray.Instance),
            KnownNameKeys.DeviceRGB => new(DeviceRgb.Instance),
            KnownNameKeys.DeviceCMYK => new(DeviceCmyk.Instance),
           // KnownNameKeys.DeviceCMYK => CreateCmykColorSpace(),
            _ => throw new PdfParseException("Unrecognized colorspace")
        };
    }

    private static IColorSpace? cmykColorSpacel;
    private static async ValueTask<IColorSpace> CreateCmykColorSpace() => cmykColorSpacel ??= 
        new IccColorSpace((await ReadCmyk()).TransformTo(await ReadSrgb()));
    private static IccProfile? sRGB;
    private static async ValueTask<IccProfile> ReadSrgb() => sRGB ??=
        await LoadProfile(@"C:\Users\jmelv\Documents\Scratch\sRGB_v4_ICC_preference.icc");

    private static IccProfile? cmyk;
    private static async ValueTask<IccProfile> ReadCmyk() => cmyk ??=
        await LoadProfile(@"C:\Users\jmelv\Documents\Scratch\ICC_Profile_Registry_01_11_2013\CGATS21_CRPC3.icc");
#warning -- obviously cannot source profiles off of my local hard drive. -- also need to consider render intents.

    
    private static ValueTask<IccProfile> LoadProfile(string profileFile) => 
        new IccParser(PipeReader.Create(File.OpenRead(profileFile))).ParseAsync();

}