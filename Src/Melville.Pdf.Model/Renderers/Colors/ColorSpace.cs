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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            KnownNameKeys.DeviceCMYK => CreateCmykColorSpace(),
            _ => throw new PdfParseException("Unrecognized colorspace")
        };
    }

    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpace() => cmykColorSpacel ??= 
        new IccColorSpace((await ReadCmyk()).TransformTo(await ReadSrgb()));
    private static IccProfile? sRGB;
    private static async ValueTask<IccProfile> ReadSrgb() => sRGB ??=
        await LoadProfile(@"AdobeSrgb.icc");

    private static IccProfile? cmyk;
    private static async ValueTask<IccProfile> ReadCmyk() => cmyk ??=
        await LoadProfile(@"Cmyk.icc");

    
    private static ValueTask<IccProfile> LoadProfile(string profileFile) =>
        new IccParser(PipeReader.Create(
            GetIccProfileData(profileFile))).ParseAsync();

    private static Stream GetIccProfileData(string profileFile) =>
        typeof(ColorSpaceFactory).Assembly.GetManifestResourceStream(
            "Melville.Pdf.Model.Renderers.Colors.Profiles."+profileFile) ??
        throw new InvalidDataException("Cannot find resource: " + profileFile);
}